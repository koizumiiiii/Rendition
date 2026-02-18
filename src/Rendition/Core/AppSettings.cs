using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Rendition.Core;

public sealed class AppSettings
{
    [JsonPropertyName("modelPath")]
    public string ModelPath { get; set; } = string.Empty;

    [JsonPropertyName("defaultTargetLanguage")]
    public string DefaultTargetLanguage { get; set; } = "English";

    [JsonPropertyName("defaultFlavor")]
    public string DefaultFlavor { get; set; } = "Casual";

    [JsonPropertyName("gpuLayerCount")]
    public int GpuLayerCount { get; set; } = 35;

    [JsonPropertyName("contextSize")]
    public int ContextSize { get; set; } = 4096;

    [JsonPropertyName("maxTokens")]
    public int MaxTokens { get; set; } = 1024;

    [JsonPropertyName("temperature")]
    public float Temperature { get; set; } = 0.7f;

    [JsonPropertyName("topP")]
    public float TopP { get; set; } = 0.9f;

    [JsonPropertyName("supportedLanguages")]
    public List<string> SupportedLanguages { get; set; } =
    [
        "English", "Japanese", "Chinese", "Korean",
        "French", "German", "Spanish", "Portuguese",
        "Italian", "Russian"
    ];

    private static readonly JsonSerializerOptions s_jsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    public static AppSettings Load(string path)
    {
        if (!File.Exists(path))
            return new AppSettings();

        var json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<AppSettings>(json, s_jsonOptions) ?? new AppSettings();
    }

    public void Save(string path)
    {
        var dir = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(dir))
            Directory.CreateDirectory(dir);

        var json = JsonSerializer.Serialize(this, s_jsonOptions);
        File.WriteAllText(path, json);
    }
}
