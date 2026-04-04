namespace DreadRemoteConnector.Packets.Receiving;

internal class HandshakeReceivePacket : IReceivePacketWithType
{
	public static PacketType PacketType => PacketType.Handshake;

	public bool VerifyRequestNumber => true;

	public ValueTask ReceiveAsync(ReceiveContext context, CancellationToken cancellationToken)
		=> default;
}