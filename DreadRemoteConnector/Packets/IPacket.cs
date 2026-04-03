namespace DreadRemoteConnector.Packets;

internal interface IPacket
{
    static abstract PacketType PacketType { get; }
}

internal interface ISendPacket : IPacket
{
    void WriteTo(BinaryWriter writer);
}

internal interface IReceivePacket : IPacket
{
    static abstract bool VerifyRequestNumber { get; }

    ValueTask ReceiveAsync(ReceiveContext context, CancellationToken cancellationToken);
}