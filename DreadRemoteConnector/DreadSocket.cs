using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Sockets;
using DreadRemoteConnector.Packets;
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

            // Send handshake with interests, and wait for response
            await SendPacketAsync(new HandshakeSendPacket(ConnectionInterests), cancellationToken).ConfigureAwait(false);
            await ReceivePacketAsync<HandshakeReceivePacket>(cancellationToken);
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

    private async Task SendPacketAsync<TPacket>(TPacket packet, CancellationToken cancellationToken)
    where TPacket : ISendPacket
    {
        CheckConnected();

        int packetLength;

        await using (var bufferStream = new MemoryStream(this.buffer))
        {
            await using var writer = new BinaryWriter(bufferStream);

            // Write the packet type first
            writer.Write((byte) TPacket.PacketType);

            // Then write the actual packet data
            packet.WriteTo(writer);

            packetLength = (int) bufferStream.Length;
        }

        // Send the packet
        var packetData = this.buffer.AsMemory()[..packetLength];
        var bytesSent = await this.socket.SendAsync(packetData, cancellationToken).ConfigureAwait(false);

        Debug.Assert(bytesSent == packetLength);
    }

    private async Task<TPacket> ReceivePacketAsync<TPacket>(CancellationToken cancellationToken)
    where TPacket : IReceivePacket
    {
        CheckConnected();

        // First, try and receive the single packet type byte
        var receiveContext = new ReceiveContext(this.socket, this.buffer);
        PacketType packetType;

        using (var reader = await receiveContext.ReadChunkAsync(1, cancellationToken).ConfigureAwait(false))
            packetType = (PacketType) reader.ReadByte();

        if (packetType != TPacket.PacketType)
            throw new IOException($"Expected packet type \"{TPacket.PacketType}\" ({(char) TPacket.PacketType}) " +
                                  $"but got \"{packetType}\" ({(char) packetType}) instead");

        var packet = CreateReceivePacket(packetType);

        Debug.Assert(packet is TPacket, "Wrong packet type!");

        if (TPacket.VerifyRequestNumber)
            await CheckRequestNumberAsync(cancellationToken).ConfigureAwait(false);

        await packet.ReceiveAsync(receiveContext, cancellationToken).ConfigureAwait(false);

        return (TPacket) packet;
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

    private static IReceivePacket CreateReceivePacket(PacketType packetType)
        => packetType switch {
            PacketType.Handshake => new HandshakeReceivePacket(),

            _ => throw new ArgumentException($"Unknown {nameof(PacketType)}: {packetType}", nameof(packetType)),
        };
}