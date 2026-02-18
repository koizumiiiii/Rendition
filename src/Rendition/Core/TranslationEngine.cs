using System.Text;
using LLama;
using LLama.Common;
using LLama.Sampling;

namespace Rendition.Core;

public sealed class TranslationEngine : IDisposable
{
    private LLamaWeights? _model;
    private LLamaContext? _context;
    private InteractiveExecutor? _executor;
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

        var parameters = new ModelParams(modelPath)
        {
            GpuLayerCount = _settings.GpuLayerCount,
            ContextSize = (uint)_settings.ContextSize,
        };

        _model = await LLamaWeights.LoadFromFileAsync(parameters, ct);
        _context = _model.CreateContext(parameters);
        _executor = new InteractiveExecutor(_context);

        progress?.Report("Model loaded successfully.");
    }

    public async Task<string> TranslateAsync(
        string inputText,
        string targetLanguage,
        Flavor flavor,
        CancellationToken ct = default)
    {
        if (_model == null || _executor == null)
            throw new InvalidOperationException("Model is not loaded. Call LoadModelAsync first.");

        if (string.IsNullOrWhiteSpace(inputText))
            return string.Empty;

        var chatHistory = new ChatHistory();
        chatHistory.AddMessage(AuthorRole.System, flavor.SystemPrompt);

        var userPrompt = $"Translate the following text to {targetLanguage}:\n\n{inputText}";

        var session = new ChatSession(_executor, chatHistory);

        var inferenceParams = new InferenceParams
        {
            MaxTokens = _settings.MaxTokens,
            SamplingPipeline = new DefaultSamplingPipeline
            {
                Temperature = _settings.Temperature,
                TopP = _settings.TopP,
            },
            AntiPrompts = ["<|im_end|>", "User:", "\n\nNote:", "\n\nAlternative:"],
        };

        var sb = new StringBuilder();

        await foreach (var token in session.ChatAsync(
            new ChatHistory.Message(AuthorRole.User, userPrompt),
            inferenceParams, ct))
        {
            sb.Append(token);
        }

        return CleanOutput(sb.ToString());
    }

    private static string CleanOutput(string raw)
    {
        var result = raw.Trim();

        // Remove common artifacts
        if (result.StartsWith("Assistant:", StringComparison.OrdinalIgnoreCase))
            result = result["Assistant:".Length..].Trim();

        // Remove trailing stop tokens
        var stopTokens = new[] { "<|im_end|>", "<|im_start|>" };
        foreach (var token in stopTokens)
        {
            var idx = result.IndexOf(token, StringComparison.Ordinal);
            if (idx >= 0)
                result = result[..idx].Trim();
        }

        return result;
    }

    private void Unload()
    {
        _executor = null;
        _context?.Dispose();
        _context = null;
        _model?.Dispose();
        _model = null;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        Unload();
    }
}
