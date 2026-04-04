namespace DreadRemoteConnector.Packets.Receiving;

internal class HandshakeReceivePacket : IReceivePacket
{
	public bool VerifyRequestNumber => true;

	public ValueTask ReceiveAsync(ReceiveContext context, CancellationToken cancellationToken)
		=> default;
}