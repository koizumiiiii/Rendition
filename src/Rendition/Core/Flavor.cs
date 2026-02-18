using System.Text.Json.Serialization;

namespace Rendition.Core;

public sealed class Flavor
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("systemPrompt")]
    public string SystemPrompt { get; set; } = string.Empty;

    public override string ToString() => Name;
}

public sealed class FlavorCollection
{
    [JsonPropertyName("flavors")]
    public List<Flavor> Flavors { get; set; } = [];
}
