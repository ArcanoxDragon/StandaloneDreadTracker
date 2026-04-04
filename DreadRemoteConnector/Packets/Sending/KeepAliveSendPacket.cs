namespace DreadRemoteConnector.Packets.Sending;

internal class KeepAliveSendPacket : ISendPacketWithType
{
	public static PacketType PacketType => PacketType.KeepAlive;

	public void WriteTo(BinaryWriter writer)
	{
		// Empty packet
	}
}