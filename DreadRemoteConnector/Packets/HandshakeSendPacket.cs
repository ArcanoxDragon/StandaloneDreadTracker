namespace DreadRemoteConnector.Packets;

internal class HandshakeSendPacket(ConnectionInterests interests) : ISendPacket
{
	public static PacketType PacketType => PacketType.Handshake;

	public void WriteTo(BinaryWriter writer)
	{
		writer.Write((byte) interests);
	}
}