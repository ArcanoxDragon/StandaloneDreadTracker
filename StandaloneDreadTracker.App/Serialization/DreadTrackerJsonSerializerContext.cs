using System.Text.Json;
using System.Text.Json.Serialization;

namespace StandaloneDreadTracker.App.Serialization;

[JsonSourceGenerationOptions(JsonSerializerDefaults.Web, WriteIndented = true, Converters = [typeof(JsonStringEnumConverter)])]
[JsonSerializable(typeof(object))] // TODO:
internal partial class DreadTrackerJsonSerializerContext : JsonSerializerContext;