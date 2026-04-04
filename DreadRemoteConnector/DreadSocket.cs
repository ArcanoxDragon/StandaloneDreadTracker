using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Sockets;
using System.Text;
using DreadRemoteConnector.Lua;
using DreadRemoteConnector.Packets;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace DreadRemoteConnector;

[PublicAPI]
public sealed partial class DreadSocket : IDisposable
{
	private const int DefaultPort           = 6969; // extra nice
	private const int DefaultBufferSize     = 4096;
	private const int SendTimeoutSeconds    = 30;
	private const int ReceiveTimeoutSeconds = 15;

	// Custom UTF8Encoding that does not write BOM
	private static readonly UTF8Encoding TextEncoding = new(encoderShouldEmitUTF8Identifier: false);

	private readonly IPAddress ipAddress;
	private readonly int       port;

	private byte[]  buffer;
	private Socket? socket;

	public DreadSocket(IPAddress ipAddress, int port = DefaultPort)
	{
		if (ipAddress.AddressFamily != AddressFamily.InterNetwork)
			throw new ArgumentException("Only IPv4 addresses are supported", nameof(ipAddress));

		this.ipAddress = ipAddress;
		this.port = port;

		BufferSize = DefaultBufferSize;
	}

	public DreadSocket(string ipAddress, int port = DefaultPort)
		: this(IPAddress.Parse(ipAddress), port) { }

	public ConnectionInterests ConnectionInterests
	{
		get;
		set
		{
			if (IsConnected)
				throw new InvalidOperationException($"{nameof(ConnectionInterests)} may only be set before a connection is established");

			field = value;
		}
	} = ConnectionInterests.Multiworld;

	[MemberNotNullWhen(true, nameof(socket))]
	public bool IsConnected => this.socket is { Connected: true };

	public GameDetails GameDetails { get; private set; } = new();

	public ILogger? Logger
	{
		get => Log.loggerInstance;
		set => Log.loggerInstance = value;
	}

	private int BufferSize
	{
		get;
		[MemberNotNull(nameof(buffer))]
		set
		{
			if (field == value && this.buffer != null)
				return;

			field = value;
			this.buffer = new byte[value];
		}
	}

	private int RequestNumber
	{
		get;
		set => field = value % 256;
	}

	public async Task ConnectAsync(CancellationToken cancellationToken = default)
	{
		if (IsConnected)
			return;

		try
		{
			Log.Connecting(this.ipAddress, this.port);
			CreateSocket();
			await this.socket.ConnectAsync(this.ipAddress, this.port, cancellationToken).ConfigureAwait(false);
			Log.ConnectionSucceeded(ConnectionInterests);

			// Send handshake with interests, and wait for response
			await SendPacketAsync(new HandshakeSendPacket(ConnectionInterests), cancellationToken).ConfigureAwait(false);
			await ReceivePacketAsync<HandshakeReceivePacket>(cancellationToken).ConfigureAwait(false);

			// Get game details
			var gameDetailsCode = LuaSnippets.GetSnippet(LuaSnippets.SnippetNames.GetGameDetails);
			var gameDetailsResponse = await ExecuteLuaAsync(gameDetailsCode, cancellationToken).ConfigureAwait(false);

			GameDetails = GameDetails.Parse(gameDetailsResponse);
			Log.GotGameDetails(GameDetails);

			// Update our own buffer size to match the game's (if it differs)
			BufferSize = GameDetails.BufferSize;
		}
		catch (OperationCanceledException)
		{
			// Don't log these
			DestroySocket();
			throw;
		}
		catch (Exception ex)
		{
			DestroySocket();
			Log.ConnectionFailed(ex, this.ipAddress, this.port);
			throw;
		}
	}

	public async Task<string> ExecuteLuaAsync(string luaCode, CancellationToken cancellationToken = default)
	{
		CheckConnected();

		const int LengthPrefixBytes = 4;
		var maxCodeByteLength = BufferSize - LengthPrefixBytes;
		var codeByteLength = TextEncoding.GetByteCount(luaCode);

		if (codeByteLength > maxCodeByteLength)
			throw new ArgumentException($"Code is too long! Size of code may not be larger than {maxCodeByteLength} bytes.", nameof(luaCode));

		await SendPacketAsync(new ExecuteLuaSendPacket(luaCode), cancellationToken).ConfigureAwait(false);

		var responsePacket = await ReceivePacketAsync<ExecuteLuaReceivePacket>(cancellationToken).ConfigureAwait(false);

		if (!responsePacket.Success)
			throw new DreadLuaException("The Lua script encountered an error", responsePacket.Response);

		return responsePacket.Response;
	}

	private async Task SendPacketAsync<TPacket>(TPacket packet, CancellationToken cancellationToken)
	where TPacket : ISendPacket
	{
		CheckConnected();

		int packetLength;

		await using (var bufferStream = new MemoryStream(this.buffer))
		{
			await using var writer = new BinaryWriter(bufferStream, TextEncoding);

			// Write the packet type first
			writer.Write((byte) TPacket.PacketType);

			// Then write the actual packet data
			packet.WriteTo(writer);

			packetLength = (int) bufferStream.Position;
		}

		// Send the packet
		var packetData = this.buffer.AsMemory()[..packetLength];
		var bytesSent = await this.socket.SendAsync(packetData, cancellationToken).ConfigureAwait(false);

		Debug.Assert(bytesSent == packetLength);
	}

	private Task<TPacket> ReceivePacketAsync<TPacket>(CancellationToken cancellationToken)
	where TPacket : IReceivePacket, new()
		=> ReceivePacketAsync<TPacket>(handleMalformed: true, cancellationToken);

	private async Task<TPacket> ReceivePacketAsync<TPacket>(bool handleMalformed, CancellationToken cancellationToken)
	where TPacket : IReceivePacket, new()
	{
		CheckConnected();

		// First, try and receive the single packet type byte
		var receiveContext = new ReceiveContext(this.socket, this.buffer, TextEncoding);
		PacketType packetType;

		using (var reader = await receiveContext.ReadChunkAsync(1, cancellationToken).ConfigureAwait(false))
			packetType = (PacketType) reader.ReadByte();

		if (handleMalformed && packetType == PacketType.MalformedPacket)
			await HandleMalformedPacketAsync(cancellationToken).ConfigureAwait(false);
		else if (packetType != TPacket.PacketType)
			throw new IOException($"Expected packet type \"{TPacket.PacketType}\" ({(char) TPacket.PacketType}) " +
								  $"but got \"{packetType}\" ({(char) packetType}) instead");

		var packet = new TPacket();

		if (TPacket.VerifyRequestNumber)
			await CheckRequestNumberAsync(cancellationToken).ConfigureAwait(false);

		await packet.ReceiveAsync(receiveContext, cancellationToken).ConfigureAwait(false);

		return packet;
	}

	private async Task CheckRequestNumberAsync(CancellationToken cancellationToken)
	{
		CheckConnected();

		var receiveData = this.buffer.AsMemory(0, 1);
		var bytesReceived = await this.socket.ReceiveAsync(receiveData, cancellationToken).ConfigureAwait(false);

		if (bytesReceived != 1)
			throw new IOException("Packet header was empty");

		if (this.buffer[0] != RequestNumber)
			throw new IOException($"Expected request {RequestNumber} but got {this.buffer[0]}");

		RequestNumber++;
	}

	[DoesNotReturn]
	private async Task HandleMalformedPacketAsync(CancellationToken cancellationToken)
	{
		var malformedPacketInfo = await ReceivePacketAsync<MalformedPacketReceivePacket>(handleMalformed: false, cancellationToken).ConfigureAwait(false);

		Log.MalformedPacket(malformedPacketInfo);

		throw new DreadLuaException("The game received a malformed packet! The protocol may have changed.");
	}

	[MemberNotNull(nameof(socket))]
	private void CreateSocket()
	{
		this.socket?.Dispose();
		this.socket = new Socket(SocketType.Stream, ProtocolType.Tcp) {
			SendTimeout = SendTimeoutSeconds * 1000,
			ReceiveTimeout = ReceiveTimeoutSeconds * 1000,
		};

		RequestNumber = 0;
	}

	private void DestroySocket()
	{
		this.socket?.Dispose();
		this.socket = null;
	}

	[MemberNotNull(nameof(socket))]
	private void CheckConnected()
	{
		if (!IsConnected)
			throw new InvalidOperationException("Socket is not connected");
	}

	#region IDisposable

	private bool disposed;

	public void Dispose() => Dispose(true);

	private void Dispose(bool disposing)
	{
		if (Interlocked.Exchange(ref this.disposed, true))
			return;

		if (disposing)
			DestroySocket();
	}

	#endregion
}