using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Rendition.Core;

namespace Rendition.ViewModels;

public partial class MainViewModel : ObservableObject, IDisposable
{
    private readonly TranslationEngine _engine;
    private readonly FlavorManager _flavorManager = new();
    private readonly AppSettings _settings;
    private readonly string _settingsPath;
    private CancellationTokenSource? _cts;

    [ObservableProperty]
    private string _inputText = string.Empty;

    [ObservableProperty]
    private string _outputText = string.Empty;

    [ObservableProperty]
    private string _statusText = "Ready";

    [ObservableProperty]
    private bool _isTranslating;

    [ObservableProperty]
    private bool _isModelLoaded;

    [ObservableProperty]
    private string _selectedTargetLanguage = "English";

    [ObservableProperty]
    private Flavor? _selectedFlavor;

    public ObservableCollection<string> TargetLanguages { get; } = [];
    public ObservableCollection<Flavor> Flavors { get; } = [];

    public MainViewModel()
    {
        var configDir = Path.Combine(AppContext.BaseDirectory, "Config");
        _settingsPath = Path.Combine(configDir, "settings.json");

        _settings = AppSettings.Load(_settingsPath);
        _engine = new TranslationEngine(_settings);

        // Load flavors
        var flavorsPath = Path.Combine(configDir, "flavors.json");
        _flavorManager.Load(flavorsPath);
        foreach (var f in _flavorManager.Flavors)
            Flavors.Add(f);

        // Load languages
        foreach (var lang in _settings.SupportedLanguages)
            TargetLanguages.Add(lang);

        // Set defaults
        SelectedTargetLanguage = _settings.DefaultTargetLanguage;
        SelectedFlavor = _flavorManager.GetByName(_settings.DefaultFlavor) ?? Flavors.FirstOrDefault();
    }

    [RelayCommand]
    private async Task LoadModelAsync()
    {
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Title = "Select GGUF Model File",
            Filter = "GGUF Files (*.gguf)|*.gguf|All Files (*.*)|*.*",
            DefaultExt = ".gguf"
        };

        if (!string.IsNullOrEmpty(_settings.ModelPath) && File.Exists(_settings.ModelPath))
            dialog.InitialDirectory = Path.GetDirectoryName(_settings.ModelPath);

        if (dialog.ShowDialog() != true)
            return;

        var modelPath = dialog.FileName;
        StatusText = "Loading model...";
        IsTranslating = true;

        try
        {
            var progress = new Progress<string>(msg => StatusText = msg);
            await _engine.LoadModelAsync(modelPath, progress);

            _settings.ModelPath = modelPath;
            _settings.Save(_settingsPath);

            IsModelLoaded = true;
            StatusText = $"Model loaded: {Path.GetFileName(modelPath)}";
        }
        catch (Exception ex)
        {
            StatusText = $"Error: {ex.Message}";
            IsModelLoaded = false;
        }
        finally
        {
            IsTranslating = false;
        }
    }

    [RelayCommand]
    private async Task AutoLoadModelAsync()
    {
        if (string.IsNullOrEmpty(_settings.ModelPath) || !File.Exists(_settings.ModelPath))
            return;

        StatusText = "Loading model...";
        IsTranslating = true;

        try
        {
            var progress = new Progress<string>(msg => StatusText = msg);
            await _engine.LoadModelAsync(_settings.ModelPath, progress);
            IsModelLoaded = true;
            StatusText = $"Model loaded: {Path.GetFileName(_settings.ModelPath)}";
        }
        catch (Exception ex)
        {
            StatusText = $"Auto-load failed: {ex.Message}";
            IsModelLoaded = false;
        }
        finally
        {
            IsTranslating = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanTranslate))]
    private async Task TranslateAsync()
    {
        if (SelectedFlavor == null) return;

        _cts?.Cancel();
        _cts = new CancellationTokenSource();

        IsTranslating = true;
        OutputText = string.Empty;
        StatusText = "Translating...";

        try
        {
            var result = await _engine.TranslateAsync(
                InputText,
                SelectedTargetLanguage,
                SelectedFlavor,
                _cts.Token);

            OutputText = result;
            StatusText = "Done";
        }
        catch (OperationCanceledException)
        {
            StatusText = "Cancelled";
        }
        catch (Exception ex)
        {
            StatusText = $"Error: {ex.Message}";
        }
        finally
        {
            IsTranslating = false;
        }
    }

    private bool CanTranslate() =>
        IsModelLoaded && !IsTranslating && !string.IsNullOrWhiteSpace(InputText);

    partial void OnInputTextChanged(string value) => TranslateCommand.NotifyCanExecuteChanged();
    partial void OnIsModelLoadedChanged(bool value) => TranslateCommand.NotifyCanExecuteChanged();
    partial void OnIsTranslatingChanged(bool value) => TranslateCommand.NotifyCanExecuteChanged();

    [RelayCommand]
    private void CopyOutput()
    {
        if (!string.IsNullOrEmpty(OutputText))
        {
            Clipboard.SetText(OutputText);
            StatusText = "Copied to clipboard!";
        }
    }

    [RelayCommand]
    private void PasteInput()
    {
        if (Clipboard.ContainsText())
        {
            InputText = Clipboard.GetText();
        }
    }

    [RelayCommand]
    private void SwapLanguages()
    {
        // Swap input/output and try to detect/switch target language
        if (!string.IsNullOrEmpty(OutputText))
        {
            (InputText, OutputText) = (OutputText, string.Empty);
        }
    }

    public void Dispose()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _engine.Dispose();
    }
}
