using System.Net.Sockets;
using System.Text;

namespace DreadRemoteConnector;

internal class ReceiveContext(Socket socket, byte[] buffer, Encoding encoding)
{
	public Encoding Encoding { get; } = encoding;

    public async Task<BinaryReader> ReadChunkAsync(int length, CancellationToken cancellationToken)
    {
        var chunkData = buffer.AsMemory(0, length);
        var bytesReceived = await socket.ReceiveAsync(chunkData, cancellationToken).ConfigureAwait(false);

        if (bytesReceived != length)
            throw new IOException($"Expected to read {length} bytes, but {bytesReceived} were received");

        var chunkStream = new MemoryStream(buffer, 0, bytesReceived, false);

        return new BinaryReader(chunkStream, Encoding);
    }
}