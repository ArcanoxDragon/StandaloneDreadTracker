using System.Text.Json.Serialization;

namespace DreadRemoteConnector.Serialization;

[JsonSerializable(typeof(int[]))]
internal partial class DreadConnectorJsonSerializerContext : JsonSerializerContext;