using System.Buffers;
using System.Text;

namespace DreadRemoteConnector.Extensions;

internal static class BinaryReaderExtensions
{
	public static string ReadStringFast(this BinaryReader reader, int byteLength, Encoding encoding)
	{
		var responseBuffer = ArrayPool<byte>.Shared.Rent(byteLength);
		var responseData = responseBuffer.AsSpan(0, byteLength);

		try
		{
			reader.ReadExactly(responseData);

			return encoding.GetString(responseData);
		}
		finally
		{
			ArrayPool<byte>.Shared.Return(responseBuffer);
		}
	}
}