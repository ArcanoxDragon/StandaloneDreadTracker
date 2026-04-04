namespace DreadRemoteConnector;

[Flags]
public enum ConnectionInterests : byte
{
	None,
	Logging    = 1 << 0,
	Multiworld = 1 << 1,
}