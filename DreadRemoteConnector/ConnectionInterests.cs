namespace DreadRemoteConnector;

[Flags]
public enum ConnectionInterests : byte
{
	None,
	Multiworld = 1 << 0,
	Logging    = 1 << 1,
}