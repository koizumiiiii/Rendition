using System.IO;
using System.Text.Json;

namespace Rendition.Core;

public sealed class FlavorManager
{
    private readonly List<Flavor> _flavors = [];

    public IReadOnlyList<Flavor> Flavors => _flavors;

    public void Load(string path)
    {
        _flavors.Clear();

        if (!File.Exists(path))
        {
            LoadDefaults();
            return;
        }

        var json = File.ReadAllText(path);
        var collection = JsonSerializer.Deserialize<FlavorCollection>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (collection?.Flavors is { Count: > 0 })
            _flavors.AddRange(collection.Flavors);
        else
            LoadDefaults();
    }

    public Flavor? GetByName(string name) =>
        _flavors.Find(f => f.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

    private void LoadDefaults()
    {
        _flavors.Add(new Flavor
        {
            Name = "Casual",
            Description = "Friendly, natural conversational tone",
            SystemPrompt = "You are a translation assistant. Translate the given text naturally and casually. Output ONLY the translated text."
        });
        _flavors.Add(new Flavor
        {
            Name = "Technical",
            Description = "Concise, precise, engineer-like tone",
            SystemPrompt = "You are a translation assistant. Translate the given text in a technical, precise manner. Output ONLY the translated text."
        });
    }
}
