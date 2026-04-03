using System.Buffers;
using System.Buffers.Binary;

namespace DreadRemoteConnector.Packets;

internal class ExecuteLuaReceivePacket : IReceivePacket
{
	public static PacketType PacketType          => PacketType.ExecuteRemoteLua;
	public static bool       VerifyRequestNumber => true;

	public bool   Success  { get; private set; }
	public string Response { get; private set; } = string.Empty;

	public async ValueTask ReceiveAsync(ReceiveContext context, CancellationToken cancellationToken)
	{
		int responseLength;

		// Read success flag and length of response
		using (var reader = await context.ReadChunkAsync(4, cancellationToken).ConfigureAwait(false))
		{
			// First byte is just 0 or 1 for success
			Success = reader.ReadBoolean();

			Span<byte> lengthBuffer = stackalloc byte[4];

			// Next 3 bytes are 24-bit length, stored little endian. We convert to int32 by ensuring the highest byte is 0.
			reader.ReadExactly(lengthBuffer[..3]);
			lengthBuffer[^1] = 0;
			responseLength = BinaryPrimitives.ReadInt32LittleEndian(lengthBuffer);
		}

		// Read response itself
		using (var reader = await context.ReadChunkAsync(responseLength, cancellationToken).ConfigureAwait(false))
		{
			var responseBuffer = ArrayPool<byte>.Shared.Rent(responseLength);
			var responseData = responseBuffer.AsSpan(0, responseLength);

			try
			{
				reader.ReadExactly(responseData);
				Response = context.Encoding.GetString(responseData);
			}
			finally
			{
				ArrayPool<byte>.Shared.Return(responseBuffer);
			}
		}
	}
}