# Prompter.AI.Edge — Coding Standards

## C# conventions

- C# 12, net8.0, nullable enabled
- File-scoped namespaces: `namespace Prompter.AI.Edge.Services;`
- `sealed` on service implementations
- `required` on init-only model properties
- `record` for immutable DTOs
- `IAsyncEnumerable<T>` for streaming, always with `[EnumeratorCancellation]`

## Package rules

- Keep framework-agnostic — no MAUI, Xamarin, or platform dependencies
- Every public API has an interface in `Interfaces/`
- `Models/` free of LLamaSharp types — plain C# DTOs only
- `ILogger<T>` for logging via constructor injection
- `SemaphoreSlim` for thread safety (`ChatEngine`)
- `Channel<T>` for async progress streaming (`ModelManager`)

## LLamaSharp 0.26.0

- `ModelParams.Threads` → `int` (not `uint`)
- `DefaultSamplingPipeline { Temperature, TopP, RepeatPenalty }` for sampling
- `InferenceParams` no longer has Temperature/TopP
- `LLavaWeights` removed → MTMD API (`MtmdWeights`)

## Naming

- Namespace: `Prompter.AI.Edge` (never `EdgeAI.Core`)
- DI method: `AddPrompterAIEdge()` (never `AddEdgeAI()`)
- Metadata: `prompter_model_meta.json`
