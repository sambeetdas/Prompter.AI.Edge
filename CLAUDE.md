# CLAUDE.md — Prompter.AI.Edge

## What is this?

A reusable .NET 8 NuGet package for fully offline AI operations — chat, agent skills, and image Q&A — via LLamaSharp 0.26.0 (C# bindings for llama.cpp). Framework-agnostic: usable in MAUI, Blazor, WPF, ASP.NET, console apps.

## Structure

```
src/Prompter.AI.Edge/
├── Interfaces/    IModelManager, IChatEngine, ISkillsEngine, IVisionEngine
├── Models/        ModelTier, ChatMessage, ChatRequest, DownloadProgress, ModelOptions, SkillDefinition, DownloadedModelInfo
├── Services/      ModelManager, ChatEngine, SkillsEngine, VisionEngine
└── Extensions/    ServiceCollectionExtensions → AddPrompterAIEdge(modelsDir)

tests/Prompter.AI.Edge.Tests/    xUnit + Moq
```

## Build & test

```bash
dotnet build Prompter.AI.Edge.sln
dotnet test Prompter.AI.Edge.sln
```

## Key conventions

- **Namespace**: `Prompter.AI.Edge` (never `EdgeAI.Core`)
- **DI method**: `services.AddPrompterAIEdge(modelsDirectory)`
- **Streaming**: `IAsyncEnumerable<string>` with `[EnumeratorCancellation]` on CancellationToken
- **Thread safety**: `ChatEngine` uses `SemaphoreSlim` for model access serialization
- **Downloads**: `ModelManager.DownloadFileAsync` uses `Channel<T>` (C# forbids yield in try/catch)
- **Nullable**: enabled everywhere
- **File-scoped namespaces**: `namespace Prompter.AI.Edge.Services;`
- **Sealed classes** for all service implementations

## Dependencies

| Package | Version |
|---------|---------|
| LLamaSharp | 0.26.0 |
| Microsoft.Extensions.DependencyInjection.Abstractions | 10.0.2 |
| Microsoft.Extensions.Logging.Abstractions | 10.0.2 |
| Microsoft.Extensions.Http | 10.0.2 |

## LLamaSharp 0.26.0 API notes

- `ModelParams.Threads` is `int` (not `uint`)
- Sampling via `DefaultSamplingPipeline { Temperature, TopP, RepeatPenalty }`
- `InferenceParams` no longer has Temperature/TopP — use `SamplingPipeline` property
- `LLavaWeights` removed — replaced by MTMD API (`MtmdWeights`)
- Inference: `StatelessExecutor` → `InferAsync(prompt, inferenceParams, ct)`
- Anti-prompts: `new[] { "<|end|>", "<|user|>", "</s>" }`

## Modification rules

1. Never add MAUI/Xamarin/platform dependencies — this is a net8.0 library
2. Every public API must have an interface in `Interfaces/`
3. `Models/` must stay free of LLamaSharp types — plain C# DTOs only
4. Use `ILogger<T>` for logging, injected via constructor
5. Tests need `using Xunit;` — the test project uses xUnit + Moq

## Sibling project

The MAUI mobile app (`Edge.App/`) lives in a sibling folder and references this project via relative path: `../../../Prompter.AI.Edge/src/Prompter.AI.Edge/Prompter.AI.Edge.csproj`
