namespace DreadRemoteConnector.Packets;

internal class HandshakePacket(ConnectionInterests interests) : ISendPacket
{
	public static PacketType PacketType => PacketType.Handshake;

	public void WriteTo(BinaryWriter writer)
	{
		writer.Write((byte) interests);
	}
}