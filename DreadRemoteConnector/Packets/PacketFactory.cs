using DreadRemoteConnector.Packets.Receiving;

namespace DreadRemoteConnector.Packets;

internal static class PacketFactory
{
	public static IReceivePacket CreateReceivePacket(PacketType packetType)
		=> packetType switch {
			PacketType.Handshake        => new HandshakeReceivePacket(),
			PacketType.LogMessage       => new LogMessageReceivePacket(),
			PacketType.ExecuteRemoteLua => new ExecuteLuaReceivePacket(),
			PacketType.MalformedPacket  => new MalformedPacketReceivePacket(),

			_ => throw new UnknownPacketTypeException($"Packet type \"{packetType}\" is not supported", packetType),
		};
}