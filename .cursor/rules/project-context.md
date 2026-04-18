# Prompter.AI.Edge — Cursor Context

## What is this solution?

A standalone .NET 8 NuGet package (`Prompter.AI.Edge.sln`) providing offline AI operations via LLamaSharp 0.26.0. Framework-agnostic — no MAUI or platform dependencies.

## Layout

| Folder | Namespace | Contents |
|--------|-----------|----------|
| `src/Prompter.AI.Edge/Interfaces/` | `Prompter.AI.Edge.Interfaces` | `IModelManager`, `IChatEngine`, `ISkillsEngine`, `IVisionEngine` |
| `src/Prompter.AI.Edge/Models/` | `Prompter.AI.Edge.Models` | `ModelTier`, `ChatMessage`, `ChatRequest`, `DownloadProgress`, `ModelOptions`, `SkillDefinition`, `DownloadedModelInfo` |
| `src/Prompter.AI.Edge/Services/` | `Prompter.AI.Edge.Services` | `ModelManager`, `ChatEngine`, `SkillsEngine`, `VisionEngine` |
| `src/Prompter.AI.Edge/Extensions/` | `Prompter.AI.Edge.Extensions` | `AddPrompterAIEdge()` DI extension |
| `tests/Prompter.AI.Edge.Tests/` | `Prompter.AI.Edge.Tests` | xUnit + Moq unit tests |

## Dependencies

LLamaSharp 0.26.0, Microsoft.Extensions.DependencyInjection.Abstractions 10.0.2, Microsoft.Extensions.Logging.Abstractions 10.0.2, Microsoft.Extensions.Http 10.0.2

## Build

```bash
dotnet build Prompter.AI.Edge.sln
dotnet test Prompter.AI.Edge.sln
```
