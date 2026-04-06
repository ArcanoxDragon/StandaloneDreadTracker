using System.Text.Json;
using System.Text.Json.Serialization;
using DreadRemoteConnector.Packets.Receiving;

namespace DreadRemoteConnector.Serialization;

[JsonSourceGenerationOptions(JsonSerializerDefaults.Web)]
[JsonSerializable(typeof(int[]))]
[JsonSerializable(typeof(GameStateReceivePacket.GameStateUpdate))]
internal partial class DreadConnectorJsonSerializerContext : JsonSerializerContext;