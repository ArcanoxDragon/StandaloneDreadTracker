using System.Text.Json.Serialization;
using DreadRemoteConnector.Packets.Receiving;

namespace DreadRemoteConnector.Serialization;

[JsonSerializable(typeof(int[]))]
[JsonSerializable(typeof(GameStateReceivePacket.GameStateUpdate))]
internal partial class DreadConnectorJsonSerializerContext : JsonSerializerContext;