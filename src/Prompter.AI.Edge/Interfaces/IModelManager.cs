using Prompter.AI.Edge.Models;

namespace Prompter.AI.Edge.Interfaces;

public interface IModelManager
{
    IReadOnlyList<ModelTier> GetAvailableTiers();
    Task<DownloadedModelInfo?> GetDownloadedModelAsync(CancellationToken ct = default);
    IAsyncEnumerable<DownloadProgress> DownloadModelAsync(string tierId, CancellationToken ct = default);
    Task DeleteModelAsync(CancellationToken ct = default);
    Task<bool> IsModelDownloadedAsync(CancellationToken ct = default);
    Task<string> GetModelPathAsync(CancellationToken ct = default);
    Task<string?> GetMmProjPathAsync(CancellationToken ct = default);
}
