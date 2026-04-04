namespace DreadRemoteConnector.Packets.Sending;

internal class HandshakeSendPacket(ConnectionInterests interests) : ISendPacketWithType
{
	public static PacketType PacketType => PacketType.Handshake;

	public void WriteTo(BinaryWriter writer)
	{
		writer.Write((byte) interests);
	}
}