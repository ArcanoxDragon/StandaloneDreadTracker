using System.Text;
using DreadRemoteConnector.Extensions;

namespace DreadRemoteConnector.Packets.Receiving;

public sealed class LogMessageReceivePacket : LengthPrefixedReceivePacket, IPublicReceivePacket
{
	public string Message { get; private set; } = string.Empty;

	protected override void ReadData(BinaryReader reader, int dataLength, Encoding encoding)
	{
		Message = reader.ReadStringFast(dataLength, encoding);
	}
}