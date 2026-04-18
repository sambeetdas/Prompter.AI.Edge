namespace Prompter.AI.Edge.Models;

public sealed class DownloadedModelInfo
{
    public required string TierId { get; init; }
    public required string ModelPath { get; init; }
    public string? MmProjPath { get; init; }
    public required DateTime DownloadDate { get; init; }
    public required long ModelSizeBytes { get; init; }

    public string SizeFormatted
    {
        get
        {
            const long mb = 1024L * 1024;
            const long gb = mb * 1024;
            if (ModelSizeBytes >= gb) return $"{ModelSizeBytes / (double)gb:F2} GB";
            return $"{ModelSizeBytes / (double)mb:F0} MB";
        }
    }
}
