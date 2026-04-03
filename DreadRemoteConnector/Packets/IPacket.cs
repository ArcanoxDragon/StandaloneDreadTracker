namespace DreadRemoteConnector.Packets;

public interface IPacket
{
	static abstract PacketType PacketType { get; }
}

public interface ISendPacket : IPacket
{
	void WriteTo(BinaryWriter writer);
}

public interface IReceivePacket : IPacket
{
	void ReadFrom(BinaryReader reader);
}