namespace DreadRemoteConnector.Packets.Sending;

internal interface ISendPacket
{
	void WriteTo(BinaryWriter writer);
}

internal interface ISendPacketWithType : ISendPacket, IHasPacketType;