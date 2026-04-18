using Prompter.AI.Edge.Models;

namespace Prompter.AI.Edge.Interfaces;

public interface IChatEngine
{
    Task LoadModelAsync(string modelPath, ModelOptions? options = null, CancellationToken ct = default);
    IAsyncEnumerable<string> GenerateStreamAsync(ChatRequest request, CancellationToken ct = default);
    Task<string> GenerateAsync(ChatRequest request, CancellationToken ct = default);
    void StopGeneration();
    Task UnloadModelAsync();
    bool IsLoaded { get; }
}
