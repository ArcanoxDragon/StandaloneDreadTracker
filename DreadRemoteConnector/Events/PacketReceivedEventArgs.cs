using DreadRemoteConnector.Packets.Receiving;
using JetBrains.Annotations;

namespace DreadRemoteConnector.Events;

[PublicAPI]
public class PacketReceivedEventArgs(IReceivePacket packet) : EventArgs
{
	public IReceivePacket Packet { get; } = packet;
}