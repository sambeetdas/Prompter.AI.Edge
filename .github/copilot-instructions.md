# GitHub Copilot — Prompter.AI.Edge

## Project

Reusable .NET 8 NuGet package for offline LLM inference via LLamaSharp 0.26.0. Namespace: `Prompter.AI.Edge`.

## Code style

- C# 12, net8.0, nullable enabled, file-scoped namespaces
- `sealed` service classes, `required` on model properties
- `IAsyncEnumerable<string>` for streaming with `[EnumeratorCancellation]`
- `SemaphoreSlim` for thread safety in `ChatEngine`
- `Channel<T>` for download progress (yield-in-try/catch workaround)

## Architecture

- `Interfaces/` — public contracts: `IModelManager`, `IChatEngine`, `ISkillsEngine`, `IVisionEngine`
- `Models/` — plain DTOs, no LLamaSharp types
- `Services/` — implementations: `ModelManager`, `ChatEngine`, `SkillsEngine`, `VisionEngine`
- `Extensions/` — DI: `services.AddPrompterAIEdge(modelsDir)`

## LLamaSharp 0.26.0

- Sampling: `DefaultSamplingPipeline { Temperature, TopP }` (not on `InferenceParams`)
- `ModelParams.Threads` is `int`, `GpuLayerCount` is `int`
- `LLavaWeights` removed → use MTMD API
- Executor: `StatelessExecutor` → `InferAsync()`

## Testing

xUnit + Moq. Always include `using Xunit;` in test files. Run: `dotnet test`

## Naming

- Namespace: `Prompter.AI.Edge` (not `EdgeAI.Core`)
- DI method: `AddPrompterAIEdge()` (not `AddEdgeAI()`)
- Metadata file: `prompter_model_meta.json`
