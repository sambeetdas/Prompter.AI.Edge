# Prompter.AI.Edge — AI Agent Instructions

## Purpose

This is a reusable .NET 8 NuGet package that provides offline AI operations via LLamaSharp (C# bindings for llama.cpp). It is **framework-agnostic** and can be consumed by MAUI, Blazor, WPF, ASP.NET, or console apps.

## Architecture

```
src/Prompter.AI.Edge/
├── Interfaces/          # Public API contracts
│   ├── IModelManager    # Download, delete, query GGUF models
│   ├── IChatEngine      # Load model, streaming text generation
│   ├── ISkillsEngine    # Execute built-in AI skills
│   └── IVisionEngine    # Image Q&A (multimodal)
├── Models/              # Data transfer objects
│   ├── ModelTier        # Available model definitions (Lite/Standard/Pro)
│   ├── ChatMessage      # User/Assistant/System messages
│   ├── ChatRequest      # Chat request with system prompt + messages
│   ├── DownloadProgress # Download status/progress tracking
│   ├── DownloadedModelInfo # Metadata about downloaded model
│   ├── ModelOptions     # Inference parameters (context size, temperature, etc.)
│   └── SkillDefinition  # Built-in skill definition
├── Services/            # Implementations
│   ├── ModelManager     # HTTP download with Channel<T> progress streaming
│   ├── ChatEngine       # LLamaSharp StatelessExecutor wrapper
│   ├── SkillsEngine     # 6 hardcoded skills, delegates to IChatEngine
│   └── VisionEngine     # Image Q&A, delegates to IChatEngine
└── Extensions/
    └── ServiceCollectionExtensions  # AddPrompterAIEdge(modelsDir)
```

## DI Registration

```csharp
services.AddPrompterAIEdge("/path/to/models");
```

This registers: `IModelManager` (typed HttpClient), `IChatEngine` (singleton), `ISkillsEngine` (singleton), `IVisionEngine` (singleton).

## Key patterns

- **Streaming**: All generation APIs return `IAsyncEnumerable<string>` for token-by-token output
- **Cancellation**: Every async method accepts `CancellationToken`
- **Thread safety**: `ChatEngine` uses `SemaphoreSlim` to serialize model access
- **Download pipeline**: `ModelManager.DownloadFileAsync` uses `System.Threading.Channels.Channel<DownloadProgress>` because C# forbids `yield return` in try/catch blocks

## LLamaSharp 0.26.0 API

- Load model: `LLamaWeights.LoadFromFileAsync(ModelParams, ct)`
- Create context: `model.CreateContext(modelParams)`
- Inference: `new StatelessExecutor(model, params)` → `executor.InferAsync(prompt, inferenceParams, ct)`
- Sampling: `new DefaultSamplingPipeline { Temperature = 0.7f, TopP = 0.9f }`
- Anti-prompts: `new[] { "<|end|>", "<|user|>", "</s>" }`

## Testing

```bash
dotnet test tests/Prompter.AI.Edge.Tests/
```

Tests use xUnit + Moq. When adding tests, always include `using Xunit;`.

## Rules for modifications

1. Never add MAUI, Xamarin, or platform-specific dependencies
2. Every new public API needs an interface in `Interfaces/`
3. Keep `Models/` free of LLamaSharp types — they are plain C# DTOs
4. Use `ILogger<T>` for all logging
5. Maintain backward compatibility of the DI extension method signature
