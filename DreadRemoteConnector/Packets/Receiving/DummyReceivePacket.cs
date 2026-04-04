using System.Text;

namespace DreadRemoteConnector.Packets.Receiving;

internal class DummyReceivePacket : LengthPrefixedReceivePacket
{
	protected override void ReadData(BinaryReader reader, int dataLength, Encoding encoding)
	{
		// Data is not used
	}
}