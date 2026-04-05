using System.Diagnostics.CodeAnalysis;
using DreadRemoteConnector.Packets.Receiving;

namespace DreadRemoteConnector.Packets;

internal static class PacketFactory
{
	public static bool TryCreateReceivePacket(PacketType packetType, [NotNullWhen(true)] out IReceivePacket? packet)
	{
		packet = packetType switch {
			PacketType.Handshake          => new HandshakeReceivePacket(),
			PacketType.LogMessage         => new LogMessageReceivePacket(),
			PacketType.ExecuteRemoteLua   => new ExecuteLuaReceivePacket(),
			PacketType.NewInventory       => new NewInventoryReceivePacket(),
			PacketType.CollectedLocations => new CollectedLocationsReceivePacket(),
			PacketType.GameState          => new GameStateReceivePacket(),
			PacketType.MalformedPacket    => new MalformedPacketReceivePacket(),

			_ => null,
		};
		return packet != null;
	}
}