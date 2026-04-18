using System.Runtime.CompilerServices;
using System.Text.Json;
using Prompter.AI.Edge.Interfaces;
using Prompter.AI.Edge.Models;
using Microsoft.Extensions.Logging;

namespace Prompter.AI.Edge.Services;

public sealed class ModelManager : IModelManager
{
    private const string MetadataFileName = "prompter_model_meta.json";

    private readonly HttpClient _httpClient;
    private readonly ILogger<ModelManager> _logger;
    private readonly string _modelsDirectory;
    private CancellationTokenSource? _downloadCts;

    public ModelManager(HttpClient httpClient, ILogger<ModelManager> logger, string modelsDirectory)
    {
        _httpClient = httpClient;
        _logger = logger;
        _modelsDirectory = modelsDirectory;
        Directory.CreateDirectory(_modelsDirectory);
    }

    public IReadOnlyList<ModelTier> GetAvailableTiers() => ModelTier.AvailableTiers;

    public async Task<bool> IsModelDownloadedAsync(CancellationToken ct = default)
    {
        var info = await GetDownloadedModelAsync(ct);
        return info is not null;
    }

    public async Task<DownloadedModelInfo?> GetDownloadedModelAsync(CancellationToken ct = default)
    {
        var metaPath = Path.Combine(_modelsDirectory, MetadataFileName);
        if (!File.Exists(metaPath)) return null;

        try
        {
            var json = await File.ReadAllTextAsync(metaPath, ct);
            var meta = JsonSerializer.Deserialize<ModelMetadata>(json);
            if (meta is null) return null;

            var tier = ModelTier.GetById(meta.TierId);
            if (tier is null) return null;

            var modelPath = Path.Combine(_modelsDirectory, tier.ModelFileName);
            if (!File.Exists(modelPath)) return null;

            var fileInfo = new FileInfo(modelPath);
            return new DownloadedModelInfo
            {
                TierId = meta.TierId,
                ModelPath = modelPath,
                MmProjPath = tier.MmProjFileName is not null
                    ? Path.Combine(_modelsDirectory, tier.MmProjFileName)
                    : null,
                DownloadDate = meta.DownloadDate,
                ModelSizeBytes = fileInfo.Length
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to read model metadata");
            return null;
        }
    }

    public async IAsyncEnumerable<DownloadProgress> DownloadModelAsync(
        string tierId,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        var tier = ModelTier.GetById(tierId);
        if (tier is null)
        {
            yield return DownloadProgress.Error($"Unknown model tier: {tierId}");
            yield break;
        }

        _downloadCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        var linkedCt = _downloadCts.Token;

        long totalSize = tier.TotalSizeBytes;

        yield return new DownloadProgress
        {
            Status = DownloadStatus.Downloading,
            CurrentFile = tier.ModelFileName,
            TotalBytes = totalSize
        };

        await foreach (var progress in DownloadFileAsync(
            tier.DownloadUrl,
            Path.Combine(_modelsDirectory, tier.ModelFileName),
            tier.ModelFileName,
            tier.ModelSizeBytes,
            totalSize,
            0,
            linkedCt))
        {
            yield return progress;
            if (progress.Status is DownloadStatus.Failed or DownloadStatus.Paused)
                yield break;
        }

        if (tier.MmProjFileName is not null && tier.MmProjDownloadUrl is not null)
        {
            await foreach (var progress in DownloadFileAsync(
                tier.MmProjDownloadUrl,
                Path.Combine(_modelsDirectory, tier.MmProjFileName),
                tier.MmProjFileName,
                tier.MmProjSizeBytes ?? 0,
                totalSize,
                tier.ModelSizeBytes,
                linkedCt))
            {
                yield return progress;
                if (progress.Status is DownloadStatus.Failed or DownloadStatus.Paused)
                    yield break;
            }
        }

        var meta = new ModelMetadata { TierId = tierId, DownloadDate = DateTime.UtcNow };
        var metaJson = JsonSerializer.Serialize(meta);
        await File.WriteAllTextAsync(Path.Combine(_modelsDirectory, MetadataFileName), metaJson, linkedCt);

        yield return DownloadProgress.Completed;
    }

    public async Task DeleteModelAsync(CancellationToken ct = default)
    {
        var info = await GetDownloadedModelAsync(ct);
        if (info is null) return;

        TryDeleteFile(info.ModelPath);
        if (info.MmProjPath is not null) TryDeleteFile(info.MmProjPath);
        TryDeleteFile(Path.Combine(_modelsDirectory, MetadataFileName));

        _logger.LogInformation("Model {TierId} deleted", info.TierId);
    }

    public async Task<string> GetModelPathAsync(CancellationToken ct = default)
    {
        var info = await GetDownloadedModelAsync(ct)
            ?? throw new InvalidOperationException("No model downloaded");
        return info.ModelPath;
    }

    public async Task<string?> GetMmProjPathAsync(CancellationToken ct = default)
    {
        var info = await GetDownloadedModelAsync(ct);
        return info?.MmProjPath;
    }

    private async IAsyncEnumerable<DownloadProgress> DownloadFileAsync(
        string url, string savePath, string fileName,
        long fileSizeBytes, long totalSizeBytes, long previouslyDownloaded,
        [EnumeratorCancellation] CancellationToken ct)
    {
        var channel = System.Threading.Channels.Channel.CreateUnbounded<DownloadProgress>();

        _ = Task.Run(async () =>
        {
            try
            {
                using var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, ct);
                response.EnsureSuccessStatusCode();

                await using var contentStream = await response.Content.ReadAsStreamAsync(ct);
                await using var fileStream = new FileStream(savePath, FileMode.Create, FileAccess.Write, FileShare.None, 81920, true);

                var buffer = new byte[81920];
                long fileDownloaded = 0;
                int bytesRead;

                while ((bytesRead = await contentStream.ReadAsync(buffer, ct)) > 0)
                {
                    await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead), ct);
                    fileDownloaded += bytesRead;

                    var overallDownloaded = previouslyDownloaded + fileDownloaded;
                    var overallProgress = (double)overallDownloaded / totalSizeBytes;

                    await channel.Writer.WriteAsync(new DownloadProgress
                    {
                        Status = DownloadStatus.Downloading,
                        Progress = Math.Clamp(overallProgress, 0.0, 1.0),
                        DownloadedBytes = overallDownloaded,
                        TotalBytes = totalSizeBytes,
                        CurrentFile = fileName
                    }, ct);
                }
            }
            catch (OperationCanceledException)
            {
                await channel.Writer.WriteAsync(new DownloadProgress { Status = DownloadStatus.Paused });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to download {FileName}", fileName);
                await channel.Writer.WriteAsync(DownloadProgress.Error($"Download failed: {ex.Message}"));
            }
            finally
            {
                channel.Writer.Complete();
            }
        }, ct);

        await foreach (var progress in channel.Reader.ReadAllAsync(ct))
        {
            yield return progress;
        }
    }

    private void TryDeleteFile(string path)
    {
        try { if (File.Exists(path)) File.Delete(path); }
        catch (Exception ex) { _logger.LogWarning(ex, "Failed to delete {Path}", path); }
    }

    private sealed class ModelMetadata
    {
        public string TierId { get; set; } = string.Empty;
        public DateTime DownloadDate { get; set; }
    }
}
