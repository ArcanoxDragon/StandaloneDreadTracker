namespace DreadRemoteConnector.Packets;

public enum PacketType : byte
{
	Unknown,

	// Lua needs to receive these as ASCII digits for easier parsing

	Handshake          = (byte) '1',
	LogMessage         = (byte) '2',
	ExecuteRemoteLua   = (byte) '3',
	KeepAlive          = (byte) '4',
	NewInventory       = (byte) '5',
	CollectedLocations = (byte) '6',
	ReceivedPickups    = (byte) '7',
	GameState          = (byte) '8',
	Malformed          = (byte) '9',
}