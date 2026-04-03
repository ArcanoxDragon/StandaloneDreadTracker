using System.Buffers.Binary;

namespace DreadRemoteConnector.Packets;

internal class ExecuteLuaSendPacket(string luaCode) : ISendPacket
{
	public static PacketType PacketType => PacketType.ExecuteRemoteLua;

	public void WriteTo(BinaryWriter writer)
	{
		// Write length prefix in little-endian
		Span<byte> lengthSpan = stackalloc byte[4];

		BinaryPrimitives.WriteInt32LittleEndian(lengthSpan, luaCode.Length);
		writer.Write(lengthSpan);

		// Write code itself (must use Span overload, as the string overload writes its own length prefix
		// using the current system's endianness, which could be wrong)
		writer.Write(luaCode.AsSpan());
	}
}