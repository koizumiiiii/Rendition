# Rendition

Natural, human-like translation powered by local LLM.

No more telling AI to "translate naturally" every time — Rendition bakes style instructions into presets (Flavors), so you just type and get the translation you want.

## Features

- **Flavor Presets** — Casual, Technical, Hype, Formal. Customizable via JSON.
- **Multi-language** — 18 languages supported out of the box, easily extensible.
- **Bidirectional** — Any language pair (ja→en, en→ja, zh→en, etc.)
- **Local & Offline** — Runs entirely on your machine after model download. No API keys, no cloud.
- **Uncensored Model** — Uses Qwen 2.5-1.5B Uncensored for unbiased, neutral translations.
- **GPU Accelerated** — CUDA 12 support for fast inference (~1.3GB VRAM).

## Quick Start

### 1. Download Model

Download the GGUF model file (~1.1GB):

```
https://huggingface.co/CultriX/Qwen2.5-1.5B-Instruct-Uncensored-GGUF
```

Select: `Qwen2.5-1.5B-Instruct-Uncensored-Q4_K_M.gguf`

### 2. Build & Run

```bash
dotnet build
dotnet run --project src/Rendition
```

### 3. Load Model

Click **Load Model** in the app and select the downloaded `.gguf` file. The path is saved automatically for next launch.

## Flavor Presets

| Flavor | Style | Example (ja→en) |
|--------|-------|-----------------|
| **Casual** | Friendly, conversational | "Just pushed a new update!" |
| **Technical** | Precise, engineer-like | "Implemented async dispatch for..." |
| **Hype** | Energetic, attention-grabbing | "Huge update just dropped!" |
| **Formal** | Professional, polished | "We are pleased to announce..." |

Custom flavors can be added in `Config/flavors.json`.

## Configuration

Edit `Config/settings.json` to customize:

```json
{
  "defaultTargetLanguage": "English",
  "defaultFlavor": "Casual",
  "gpuLayerCount": 35,
  "contextSize": 4096,
  "maxTokens": 1024,
  "temperature": 0.7,
  "supportedLanguages": ["English", "Japanese", "Chinese", ...]
}
```

## Tech Stack

| Component | Technology |
|-----------|-----------|
| Runtime | .NET 8 / WPF |
| LLM Engine | LLamaSharp 0.26.0 (llama.cpp) |
| GPU Backend | CUDA 12 |
| Model | Qwen 2.5-1.5B-Instruct Uncensored (GGUF Q4_K_M) |
| MVVM | CommunityToolkit.Mvvm |

## Requirements

- Windows 10/11 (x64)
- .NET 8 SDK
- NVIDIA GPU with CUDA 12 support (RTX 20xx+)
- ~1.3GB VRAM

## License

MIT
