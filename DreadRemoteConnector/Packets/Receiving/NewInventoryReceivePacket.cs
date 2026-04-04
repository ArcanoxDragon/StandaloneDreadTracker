using System.Text;
using System.Text.Json;
using DreadRemoteConnector.Extensions;
using DreadRemoteConnector.Serialization;
using JetBrains.Annotations;

namespace DreadRemoteConnector.Packets.Receiving;

[PublicAPI]
public sealed class NewInventoryReceivePacket : LengthPrefixedReceivePacket, IPublicReceivePacket
{
	public int[] ItemQuantities { get; private set; } = [];

	protected override void ReadData(BinaryReader reader, int dataLength, Encoding encoding)
	{
		var json = reader.ReadStringFast(dataLength, encoding);

		ItemQuantities = JsonSerializer.Deserialize(json, DreadConnectorJsonSerializerContext.Default.Int32Array) ?? [];
	}
}