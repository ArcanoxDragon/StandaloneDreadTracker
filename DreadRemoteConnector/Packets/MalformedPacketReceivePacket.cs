using System.Buffers;
using System.Buffers.Binary;

namespace DreadRemoteConnector.Packets;

internal class MalformedPacketReceivePacket : IReceivePacket
{
	public static PacketType PacketType          => PacketType.MalformedPacket;
	public static bool       VerifyRequestNumber => false;

	public PacketType MalformedPacketType { get; private set; }
	public int        ExpectedBytes       { get; private set; }
	public int        ReceivedBytes       { get; private set; }

	public async ValueTask ReceiveAsync(ReceiveContext context, CancellationToken cancellationToken)
	{
		using var bufferOwner = MemoryPool<byte>.Shared.Rent(9);
		var buffer = bufferOwner.Memory;

		using (var reader = await context.ReadChunkAsync(buffer.Length, cancellationToken).ConfigureAwait(false))
			reader.ReadExactly(buffer.Span);

		MalformedPacketType = (PacketType) buffer.Span[0];

		Span<byte> receivedBytesBuffer = stackalloc byte[4];
		Span<byte> expectedBytesBuffer = stackalloc byte[4];

		// "Received" and "expected" lengths are sent as 24-bit lengths, so we need to copy them
		// into 32-bit buffers and set the highest byte to 0 in order to read as an int32.
		buffer.Span[1..4].CopyTo(receivedBytesBuffer);
		receivedBytesBuffer[^1] = 0;
		buffer.Span[5..8].CopyTo(expectedBytesBuffer);
		expectedBytesBuffer[^1] = 0;

		ReceivedBytes = BinaryPrimitives.ReadInt32LittleEndian(receivedBytesBuffer);
		ExpectedBytes = BinaryPrimitives.ReadInt32LittleEndian(expectedBytesBuffer);
	}
}