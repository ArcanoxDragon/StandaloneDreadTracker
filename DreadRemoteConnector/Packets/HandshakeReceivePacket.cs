namespace DreadRemoteConnector.Packets;

internal class HandshakeReceivePacket : IReceivePacket
{
    public static PacketType PacketType          => PacketType.Handshake;
    public static bool       VerifyRequestNumber => true;

    public ValueTask ReceiveAsync(ReceiveContext context, CancellationToken cancellationToken)
        => default;
}