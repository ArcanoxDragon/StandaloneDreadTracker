namespace DreadRemoteConnector.Packets.Receiving;

internal interface IReceivePacket
{
	bool VerifyRequestNumber => false;

	ValueTask ReceiveAsync(ReceiveContext context, CancellationToken cancellationToken);
}

internal interface IReceivePacketWithType : IReceivePacket, IHasPacketType;