using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;

namespace DreadRemoteConnector;

internal sealed partial class DreadSocket : IDisposable
{
	private const int DefaultPort           = 6969; // extra nice
	private const int BufferSize            = 4096;
	private const int SendTimeoutSeconds    = 30;
	private const int ReceiveTimeoutSeconds = 15;

	private readonly byte[]    buffer;
	private readonly IPAddress ipAddress;
	private readonly int       port;

	private Socket? socket;

	public DreadSocket(IPAddress ipAddress, int port = DefaultPort)
	{
		if (ipAddress.AddressFamily != AddressFamily.InterNetwork)
			throw new ArgumentException("Only IPv4 addresses are supported", nameof(ipAddress));

		this.buffer = new byte[BufferSize];
		this.ipAddress = ipAddress;
		this.port = port;
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

	public ILogger? Logger
	{
		get => Log.loggerInstance;
		set => Log.loggerInstance = value;
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

	[MemberNotNull(nameof(socket))]
	private void CreateSocket()
	{
		this.socket?.Dispose();
		this.socket = new Socket(SocketType.Stream, ProtocolType.Tcp) {
			SendTimeout = SendTimeoutSeconds * 1000,
			ReceiveTimeout = ReceiveTimeoutSeconds * 1000,
		};
	}

	private void DestroySocket()
	{
		this.socket?.Dispose();
		this.socket = null;
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