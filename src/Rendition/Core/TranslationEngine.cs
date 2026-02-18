using System.Text;
using LLama;
using LLama.Common;
using LLama.Sampling;

namespace Rendition.Core;

public sealed class TranslationEngine : IDisposable
{
    private LLamaWeights? _model;
    private ModelParams? _modelParams;
    private readonly AppSettings _settings;
    private bool _disposed;

    public bool IsLoaded => _model != null;

    public TranslationEngine(AppSettings settings)
    {
        _settings = settings;
    }

    public async Task LoadModelAsync(string modelPath, IProgress<string>? progress = null, CancellationToken ct = default)
    {
        if (_model != null)
        {
            Unload();
        }

        progress?.Report("Loading model...");

        _modelParams = new ModelParams(modelPath)
        {
            GpuLayerCount = _settings.GpuLayerCount,
            ContextSize = (uint)_settings.ContextSize,
        };

        _model = await LLamaWeights.LoadFromFileAsync(_modelParams, ct);

        progress?.Report("Model loaded successfully.");
    }

    public async Task<string> TranslateAsync(
        string inputText,
        string targetLanguage,
        Flavor flavor,
        CancellationToken ct = default)
    {
        if (_model == null || _modelParams == null)
            throw new InvalidOperationException("Model is not loaded. Call LoadModelAsync first.");

        if (string.IsNullOrWhiteSpace(inputText))
            return string.Empty;

        // StatelessExecutor: 毎回クリーンなコンテキストで推論（翻訳間の状態汚染なし）
        var executor = new StatelessExecutor(_model, _modelParams)
        {
            ApplyTemplate = true,
            SystemMessage = flavor.SystemPrompt,
        };

        var userPrompt = $"Translate to {targetLanguage}. Preserve all meaning accurately.\n\n{inputText}";

        var inferenceParams = new InferenceParams
        {
            MaxTokens = _settings.MaxTokens,
            SamplingPipeline = new DefaultSamplingPipeline
            {
                Temperature = _settings.Temperature,
                TopP = _settings.TopP,
            },
            AntiPrompts = ["<|im_end|>", "<|im_start|>", "\n\nNote:", "\n\nAlternative:"],
        };

        var sb = new StringBuilder();

        await foreach (var token in executor.InferAsync(userPrompt, inferenceParams, ct))
        {
            sb.Append(token);
        }

        return CleanOutput(sb.ToString());
    }

    private static string CleanOutput(string raw)
    {
        var result = raw.Trim();

        // Remove trailing stop tokens
        string[] stopTokens = ["<|im_end|>", "<|im_start|>"];
        foreach (var token in stopTokens)
        {
            var idx = result.IndexOf(token, StringComparison.Ordinal);
            if (idx >= 0)
                result = result[..idx].Trim();
        }

        // Remove quotes if the entire output is wrapped in them
        if (result.Length >= 2 &&
            ((result[0] == '"' && result[^1] == '"') ||
             (result[0] == '\u201C' && result[^1] == '\u201D')))
        {
            result = result[1..^1].Trim();
        }

        return result;
    }

    private void Unload()
    {
        _model?.Dispose();
        _model = null;
        _modelParams = null;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        Unload();
    }
}
