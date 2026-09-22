using System.Text.Json.Serialization;

namespace BudeView.Core;

[JsonSerializable(typeof(TraceEvent))]
[JsonSerializable(typeof(Dictionary<string, string>))]
internal sealed partial class BudeViewJsonContext : JsonSerializerContext;
