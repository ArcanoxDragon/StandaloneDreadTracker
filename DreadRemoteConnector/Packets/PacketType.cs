namespace DreadRemoteConnector.Packets;

public enum PacketType : byte
{
	Unknown,
	Handshake          = 1,
	LogMessage         = 2,
	ExecuteRemoteLua   = 3,
	KeepAlive          = 4,
	NewInventory       = 5,
	CollectedLocations = 6,
	ReceivedPickups    = 7,
	GameState          = 8,
	MalformedPacket    = 9,
}