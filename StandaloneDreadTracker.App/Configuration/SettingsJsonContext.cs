using System.Text.Json;
using System.Text.Json.Serialization;

namespace StandaloneDreadTracker.App.Configuration;

[JsonSourceGenerationOptions(JsonSerializerDefaults.Web, WriteIndented = true)]
[JsonSerializable(typeof(ApplicationSettings))]
internal partial class SettingsJsonContext : JsonSerializerContext;