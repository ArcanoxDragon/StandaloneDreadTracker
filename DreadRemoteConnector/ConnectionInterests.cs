namespace DreadRemoteConnector;

public enum ConnectionInterests : byte
{
	None,

	// Lua needs to receive these as ASCII digits for easier parsing

	Multiworld = (byte) '1',
	Logging    = (byte) '2',
}