namespace DreadRemoteConnector.Packets;

internal interface IHasPacketType
{
	static abstract PacketType PacketType { get; }
}