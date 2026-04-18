# Prompter.AI.Edge

A reusable .NET 10 NuGet package for fully offline AI operations — chat, agent skills, and image Q&A — powered by on-device LLM inference via [LLamaSharp](https://github.com/SciSharp/LLamaSharp) (C# bindings for llama.cpp).

Framework-agnostic: usable in MAUI, Blazor, WPF, ASP.NET, or console applications.

## Features

- **Model Management** — Download, store, and delete GGUF models from HuggingFace with streaming progress updates
- **AI Chat** — Token-by-token streaming text generation via `IAsyncEnumerable<string>`
- **Agent Skills** — 6 built-in skills: Summarizer, Translator, Code Explainer, Writing Assistant, Grammar Fixer, Brainstorm Helper
- **Image Q&A** — Multimodal vision capabilities (ask questions about images)
- **Tiered Models** — Lite (500M), Standard (4B), and Pro (4B multimodal) tiers

## Architecture

```
src/Prompter.AI.Edge/
├── Interfaces/          Public API contracts
│   ├── IModelManager    Download, delete, query GGUF models
│   ├── IChatEngine      Load model, streaming text generation
│   ├── ISkillsEngine    Execute built-in AI skills
│   └── IVisionEngine    Image Q&A (multimodal)
├── Models/              Plain C# DTOs
│   ├── ModelTier        Available model definitions (Lite/Standard/Pro)
│   ├── ChatMessage      User/Assistant/System messages
│   ├── ChatRequest      Chat request with system prompt + history
│   ├── DownloadProgress Download status/progress tracking
│   ├── DownloadedModelInfo  Metadata about downloaded model
│   ├── ModelOptions     Inference parameters (context size, temperature, etc.)
│   └── SkillDefinition  Built-in skill definition
├── Services/            Implementations
│   ├── ModelManager     HTTP download with Channel<T> progress streaming
│   ├── ChatEngine       LLamaSharp StatelessExecutor wrapper
│   ├── SkillsEngine     Delegates to IChatEngine with skill-specific prompts
│   └── VisionEngine     Image Q&A, delegates to IChatEngine
└── Extensions/
    └── ServiceCollectionExtensions  → AddPrompterAIEdge(modelsDir)

tests/Prompter.AI.Edge.Tests/       xUnit + Moq unit tests
```

## Quick start

### Install

Reference the project directly or pack it as a NuGet:

```bash
dotnet pack src/Prompter.AI.Edge/Prompter.AI.Edge.csproj -c Release
```

### Register services

```csharp
using Prompter.AI.Edge.Extensions;

services.AddPrompterAIEdge("/path/to/models/directory");
```

This registers `IModelManager`, `IChatEngine`, `ISkillsEngine`, and `IVisionEngine` into your DI container.

### Use in code

```csharp
using Prompter.AI.Edge.Interfaces;
using Prompter.AI.Edge.Models;

// Download a model
var tiers = ModelTier.GetAvailableTiers();
await foreach (var progress in modelManager.DownloadModelAsync(tiers[0], ct))
    Console.Write($"\r{progress.OverallPercent:P0}");

// Load and chat
await chatEngine.LoadModelAsync(ct);

var request = new ChatRequest
{
    SystemPrompt = "You are a helpful assistant.",
    Messages = new[] { ChatMessage.FromUser("Hello!") }
};

await foreach (var token in chatEngine.GenerateStreamAsync(request, ct))
    Console.Write(token);
```

## Build & test

```bash
dotnet build Prompter.AI.Edge.sln
dotnet test Prompter.AI.Edge.sln
```

## Dependencies

| Package | Version | Purpose |
|---------|---------|---------|
| LLamaSharp | 0.26.0 | C# bindings for llama.cpp |
| Microsoft.Extensions.DependencyInjection.Abstractions | 10.0.2 | DI registration |
| Microsoft.Extensions.Logging.Abstractions | 10.0.2 | `ILogger<T>` |
| Microsoft.Extensions.Http | 10.0.2 | Typed `HttpClient` for downloads |

## Model tiers

| Tier | Model | Size | Min RAM |
|------|-------|------|---------|
| Lite | SmolVLM2-500M (Q8_0) | ~640 MB | 4 GB |
| Standard | Gemma 3 4B IT (Q4_K_M) | ~3.0 GB | 6 GB |
| Pro | Phi-4 Multimodal (Q4_K_M) | ~3.2 GB | 8 GB |

## Technical notes

- **Streaming**: All generation APIs return `IAsyncEnumerable<string>` for token-by-token output. CancellationToken is always supported via `[EnumeratorCancellation]`.
- **Thread safety**: `ChatEngine` uses `SemaphoreSlim` to serialize model access.
- **Download pipeline**: `ModelManager.DownloadFileAsync` uses `System.Threading.Channels.Channel<DownloadProgress>` because C# forbids `yield return` inside try/catch blocks.
- **LLamaSharp 0.26.0**: Sampling via `DefaultSamplingPipeline` (not `InferenceParams`). `LLavaWeights` removed in favor of MTMD API. `ModelParams.Threads` and `GpuLayerCount` are `int` (not `uint`).

## License

MIT
