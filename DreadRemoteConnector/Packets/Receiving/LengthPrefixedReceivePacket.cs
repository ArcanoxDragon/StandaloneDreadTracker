using System.Buffers.Binary;
using System.Text;

namespace DreadRemoteConnector.Packets.Receiving;

internal abstract class LengthPrefixedReceivePacket : IReceivePacket
{
	public async ValueTask ReceiveAsync(ReceiveContext context, CancellationToken cancellationToken)
	{
		int dataLength;

		// Read success length of response data
		using (var reader = await context.ReadChunkAsync(4, cancellationToken).ConfigureAwait(false))
		{
			Span<byte> lengthBuffer = stackalloc byte[4];

			reader.ReadExactly(lengthBuffer);
			dataLength = BinaryPrimitives.ReadInt32LittleEndian(lengthBuffer);
		}

		// Read actual data
		using (var reader = await context.ReadChunkAsync(dataLength, cancellationToken).ConfigureAwait(false))
			ReadData(reader, dataLength, context.Encoding);
	}

	protected abstract void ReadData(BinaryReader reader, int dataLength, Encoding encoding);
}