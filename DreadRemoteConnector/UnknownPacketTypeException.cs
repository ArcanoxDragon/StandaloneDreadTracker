using DreadRemoteConnector.Packets;

namespace DreadRemoteConnector;

public sealed class UnknownPacketTypeException(string message, PacketType packetType) : IOException(message)
{
	public PacketType PacketType { get; } = packetType;
}