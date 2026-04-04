namespace DreadRemoteConnector.Packets.Receiving;

public interface IReceivePacket
{
	bool VerifyRequestNumber => false;

	internal ValueTask ReceiveAsync(ReceiveContext context, CancellationToken cancellationToken);
}

internal interface IPublicReceivePacket; // Marker interface for raising public events