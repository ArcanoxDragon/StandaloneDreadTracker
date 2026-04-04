using System.Buffers;
using System.Text;
using JetBrains.Annotations;

namespace DreadRemoteConnector.Packets.Receiving;

// This packet type is not used by us, but might be received if we connect *after* RDV has been connected.

[PublicAPI]
public sealed class CollectedLocationsReceivePacket : LengthPrefixedReceivePacket, IPublicReceivePacket
{
	private static readonly byte[] LocationsPrefix = "locations:"u8.ToArray();

	public IReadOnlyList<int> CollectedLocations { get; private set; } = [];

	protected override void ReadData(BinaryReader reader, int dataLength, Encoding encoding)
	{
		var bufferArray = ArrayPool<byte>.Shared.Rent(dataLength);
		var buffer = bufferArray.AsSpan(0, dataLength);

		reader.ReadExactly(buffer);

		// Check for and strip off the prefix
		if (!buffer.StartsWith(LocationsPrefix))
		{
			CollectedLocations = [];
			return;
		}

		buffer = buffer[LocationsPrefix.Length..];

		// Locations are bitpacked into a little-endian byte stream,
		// with the lowest bit of the first byte representing location 0,
		// and the highest bit of the final byte representing the final location.

		var collectedLocations = new List<int>(capacity: buffer.Length * 8);
		var locationIndex = 0;

		foreach (var @byte in buffer)
		{
			for (var bit = 0; bit < 8; bit++)
			{
				if (( @byte & ( 1 << bit ) ) > 0)
					collectedLocations.Add(locationIndex);

				locationIndex++;
			}
		}

		CollectedLocations = collectedLocations;
	}
}