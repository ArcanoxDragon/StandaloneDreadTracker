using System.Text.Json;
using System.Text.Json.Serialization;

namespace StandaloneDreadTracker.App.State;

[JsonSourceGenerationOptions(JsonSerializerDefaults.Web, WriteIndented = true)]
[JsonSerializable(typeof(ApplicationState))]
internal partial class StateJsonContext : JsonSerializerContext;