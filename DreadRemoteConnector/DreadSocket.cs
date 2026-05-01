using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Sockets;
using System.Text;
using DreadRemoteConnector.Events;
using DreadRemoteConnector.Extensions;
using DreadRemoteConnector.Lua;
using DreadRemoteConnector.Packets;
using DreadRemoteConnector.Packets.Receiving;
using DreadRemoteConnector.Packets.Sending;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace DreadRemoteConnector;

[PublicAPI]
public sealed partial class DreadSocket : IDisposable
{
	public const int DefaultPort = 6969; // extra nice

	private const int DefaultBufferSize            = 4096;
	private const int DefaultSendTimeoutSeconds    = 30;
	private const int DefaultReceiveTimeoutSeconds = 15;

	// Custom UTF8Encoding that does not write BOM
	private static readonly UTF8Encoding TextEncoding = new(encoderShouldEmitUTF8Identifier: false);

	private readonly IPAddress ipAddress;
	private readonly int       port;

	private bool                     connecting;
	private byte[]                   buffer;
	private Socket?                  socket;
	private CancellationTokenSource? connectingCancelSource;
	private bool                     hasLoggedFailureSinceLastConnection;

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

	public event EventHandler? ConnectionLost;

	public event EventHandler<PacketReceivedEventArgs>? PacketReceived;

	public TimeSpan SendTimeout
	{
		get;
		set
		{
			field = value;
			this.socket?.SendTimeout = (int) value.TotalMilliseconds;
		}
	} = TimeSpan.FromSeconds(DefaultSendTimeoutSeconds);

	public TimeSpan ReceiveTimeout { get; set; } = TimeSpan.FromSeconds(DefaultReceiveTimeoutSeconds);

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
		set => Log.loggerInstance = value ?? NullLogger.Instance;
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

	public async ValueTask ConnectAsync(CancellationToken cancellationToken = default)
	{
		if (IsConnected)
			// Connection already established - don't connect again
			return;

		if (Interlocked.Exchange(ref this.connecting, true))
			// Actively in the process of connecting - prevent concurrent attempts
			return;

		using var connectingCancelSource = new CancellationTokenSource();
		using var combinedCancelSource = CancellationTokenSource.CreateLinkedTokenSource(connectingCancelSource.Token, cancellationToken);
		var combinedCancelToken = combinedCancelSource.Token;

		this.connectingCancelSource = connectingCancelSource;

		try
		{
			Log.Connecting(this.ipAddress, this.port);
			CreateSocket();
			await this.socket.ConnectAsync(this.ipAddress, this.port, combinedCancelToken).ConfigureAwait(false);
			Log.ConnectionSucceeded(ConnectionInterests);

			// Send handshake with interests, and wait for response
			await SendPacketAsync(new HandshakeSendPacket(ConnectionInterests), combinedCancelToken).ConfigureAwait(false);
			await WaitForPacketAsync<HandshakeReceivePacket>(combinedCancelToken).ConfigureAwait(false);

			// Get game details
			var gameDetailsCode = LuaSnippets.GetSnippet(LuaSnippets.SnippetNames.GetGameDetails);
			var gameDetailsResponse = await ExecuteLuaAsync(gameDetailsCode, combinedCancelToken).ConfigureAwait(false);

			GameDetails = GameDetails.Parse(gameDetailsResponse);
			Log.GotGameDetails(GameDetails);

			// Update our own buffer size to match the game's (if it differs)
			BufferSize = GameDetails.BufferSize;

			// Set up the client bootstrap code
			await SetupBootstrapAsync(combinedCancelToken).ConfigureAwait(false);

			this.hasLoggedFailureSinceLastConnection = false;
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

			if (!this.hasLoggedFailureSinceLastConnection)
			{
				Log.ConnectionFailed(ex, this.ipAddress, this.port);
				this.hasLoggedFailureSinceLastConnection = true;
			}

			throw;
		}
		finally
		{
			this.connectingCancelSource = null;
			this.connecting = false;
		}
	}

	public async ValueTask DisconnectAsync()
	{
		try
		{
			if (this.connectingCancelSource != null)
				await this.connectingCancelSource.TryCancelAsync().ConfigureAwait(false);

			if (this.socket != null)
				await this.socket.DisconnectAsync(false).ConfigureAwait(false);
		}
		catch (SocketException ex)
		{
			Log.ErrorDuringDisconnect(ex);
		}
		finally
		{
			DestroySocket();
		}
	}

	public Task<string> ExecuteLuaAsync(string luaCode, CancellationToken cancellationToken = default)
		=> ExecuteLuaAsync(luaCode, waitForResponse: true, cancellationToken)!;

	public async Task<string?> ExecuteLuaAsync(string luaCode, bool waitForResponse, CancellationToken cancellationToken = default)
	{
		CheckConnected();

		const int LengthPrefixBytes = 4;
		var maxCodeByteLength = BufferSize - LengthPrefixBytes;
		var codeByteLength = TextEncoding.GetByteCount(luaCode);

		if (codeByteLength > maxCodeByteLength)
			throw new ArgumentException($"Code is too long! Size of code may not be larger than {maxCodeByteLength} bytes.", nameof(luaCode));

		await SendPacketAsync(new ExecuteLuaSendPacket(luaCode), cancellationToken).ConfigureAwait(false);

		if (!waitForResponse)
			return null;

		var responsePacket = await WaitForPacketAsync<ExecuteLuaReceivePacket>(cancellationToken).ConfigureAwait(false);

		if (!responsePacket.Success)
			throw new DreadLuaException($"Lua script error: {responsePacket.Response}");

		return responsePacket.Response;
	}

	private async Task SetupBootstrapAsync(CancellationToken cancellationToken)
	{
		const string SuccessResult = "ok";

		Log.BeforeSendBootstrapStage(0);

		// Send stage 0, which will return a string indicating whether or not we can continue
		var stage0Code = LuaSnippets.GetSnippet(LuaSnippets.SnippetNames.BootstrapStage0);
		var stage0Result = await ExecuteLuaAsync(stage0Code, cancellationToken).ConfigureAwait(false);

		Log.AfterSendBootstrapStage(0);

		if (stage0Result != SuccessResult)
			throw new DreadLuaException($"Bootstrap failed: {stage0Result}");

		Log.BeforeSendBootstrapStage(1);

		// Send stage 1 (no need to check result)
		var stage1Code = LuaSnippets.GetSnippet(LuaSnippets.SnippetNames.BootstrapStage1);

		await ExecuteLuaAsync(stage1Code, cancellationToken).ConfigureAwait(false);

		Log.AfterSendBootstrapStage(1);
		Log.BootstrapComplete();

		// Queue an initial update
		await ExecuteLuaAsync("""Game.AddGUISF(2.0, RL.UpdateRDVClient, "")""", cancellationToken).ConfigureAwait(false);
	}

	internal async Task SendPacketAsync<TPacket>(TPacket packet, CancellationToken cancellationToken)
	where TPacket : ISendPacketWithType
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

		try
		{
			// Send the packet
			var packetData = this.buffer.AsMemory()[..packetLength];
			var bytesSent = await this.socket.SendAsync(packetData, cancellationToken).ConfigureAwait(false);

			Debug.Assert(bytesSent == packetLength);
		}
		catch (OperationCanceledException)
		{
			HandleDisconnect();
			throw;
		}
	}

	internal async Task<TPacket> WaitForPacketAsync<TPacket>(CancellationToken cancellationToken)
	where TPacket : IReceivePacket
	{
		CheckConnected();

		try
		{
			using var timeoutCancelSource = new CancellationTokenSource(ReceiveTimeout);
			using var combinedCancelSource = CancellationTokenSource.CreateLinkedTokenSource(timeoutCancelSource.Token, cancellationToken);
			var combinedToken = combinedCancelSource.Token;

			while (true)
			{
				combinedToken.ThrowIfCancellationRequested();

				var packet = await WaitForAnyPacketAsync(combinedToken).ConfigureAwait(false);

				if (packet is TPacket desiredPacket)
					return desiredPacket;
			}
		}
		catch (Exception ex) when (ex is OperationCanceledException or SocketException or IOException)
		{
			HandleDisconnect();
			throw;
		}
	}

	internal async Task<IReceivePacket> WaitForAnyPacketAsync(CancellationToken cancellationToken)
	{
		CheckConnected();

		var receiveContext = new ReceiveContext(this.socket, this.buffer, TextEncoding);

		try
		{
			// First, wait for a single byte containing a packet type
			PacketType packetType;

			using (var reader = await receiveContext.ReadChunkAsync(1, cancellationToken).ConfigureAwait(false))
				packetType = (PacketType) reader.ReadByte();

			if (packetType == PacketType.MalformedPacket)
				await HandleMalformedPacketAsync(receiveContext, cancellationToken).ConfigureAwait(false);

			if (!PacketFactory.TryCreateReceivePacket(packetType, out var packet))
			{
				// Assume that any unrecognized packet has a length prefix and data, so that we can hopefully
				// consume it "blindly" and recover the socket into a usable state again.
				var dummyPacket = new DummyReceivePacket();

				await dummyPacket.ReceiveAsync(receiveContext, cancellationToken).ConfigureAwait(false);
				throw new UnknownPacketTypeException($"Packet type \"{packetType}\" is not supported", packetType);
			}

			if (packet.VerifyRequestNumber)
				await CheckRequestNumberAsync(cancellationToken).ConfigureAwait(false);

			await packet.ReceiveAsync(receiveContext, cancellationToken).ConfigureAwait(false);

			if (packet is IPublicReceivePacket)
				PacketReceived?.Invoke(this, new PacketReceivedEventArgs(packet));

			return packet;
		}
		catch (Exception ex) when (ex is SocketException or IOException)
		{
			HandleDisconnect();
			throw;
		}
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
	private async Task HandleMalformedPacketAsync(ReceiveContext receiveContext, CancellationToken cancellationToken)
	{
		var malformedPacketInfo = new MalformedPacketReceivePacket();

		await malformedPacketInfo.ReceiveAsync(receiveContext, cancellationToken).ConfigureAwait(false);
		Log.MalformedPacket(malformedPacketInfo);

		throw new DreadLuaException("The game received a malformed packet! The protocol may have changed.");
	}

	private void HandleDisconnect()
	{
		ConnectionLost?.Invoke(this, EventArgs.Empty);
		DestroySocket();
	}

	[MemberNotNull(nameof(socket))]
	private void CreateSocket()
	{
		this.socket?.Dispose();
		this.socket = new Socket(SocketType.Stream, ProtocolType.Tcp) {
			SendTimeout = (int) SendTimeout.TotalMilliseconds,

			// The socket itself will have an infinite receive timeout to allow the event loop to wait indefinitely for new packets.
			// Receive operations with a timeout will use a timed cancellation token instead.
			ReceiveTimeout = Timeout.Infinite,
		};

		RequestNumber = 0;
	}

	private void DestroySocket()
	{
		try
		{
			this.socket?.Disconnect(false);
		}
		catch
		{
			// Ignore any exceptions here - don't care when we're destroying it anyways
		}

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