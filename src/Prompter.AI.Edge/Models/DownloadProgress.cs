namespace Prompter.AI.Edge.Models;

public enum DownloadStatus
{
    Idle,
    Downloading,
    Paused,
    Verifying,
    Completed,
    Failed
}

public sealed class DownloadProgress
{
    public DownloadStatus Status { get; init; } = DownloadStatus.Idle;
    public double Progress { get; init; }
    public long DownloadedBytes { get; init; }
    public long TotalBytes { get; init; }
    public string? CurrentFile { get; init; }
    public string? ErrorMessage { get; init; }

    public string ProgressPercent => $"{Progress * 100:F1}%";

    public static DownloadProgress Idle => new();

    public static DownloadProgress Completed => new()
    {
        Status = DownloadStatus.Completed,
        Progress = 1.0
    };

    public static DownloadProgress Error(string message) => new()
    {
        Status = DownloadStatus.Failed,
        ErrorMessage = message
    };
}
