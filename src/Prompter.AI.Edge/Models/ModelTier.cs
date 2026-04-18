namespace Prompter.AI.Edge.Models;

public sealed class ModelTier
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required string Description { get; init; }
    public required string ModelFileName { get; init; }
    public string? MmProjFileName { get; init; }
    public required string DownloadUrl { get; init; }
    public string? MmProjDownloadUrl { get; init; }
    public required long ModelSizeBytes { get; init; }
    public long? MmProjSizeBytes { get; init; }
    public required int MinRamGb { get; init; }
    public required bool SupportsVision { get; init; }

    public long TotalSizeBytes => ModelSizeBytes + (MmProjSizeBytes ?? 0);

    public string TotalSizeFormatted => FormatBytes(TotalSizeBytes);

    private static string FormatBytes(long bytes)
    {
        const long mb = 1024L * 1024;
        const long gb = mb * 1024;
        if (bytes >= gb) return $"{bytes / (double)gb:F1} GB";
        return $"{bytes / (double)mb:F0} MB";
    }

    public static IReadOnlyList<ModelTier> AvailableTiers { get; } = new[]
    {
        new ModelTier
        {
            Id = "lite",
            Name = "Lite",
            Description = "Compact model for older phones. Handles text chat and basic image Q&A.",
            ModelFileName = "smolvlm2-500m-Q8_0.gguf",
            DownloadUrl = "https://huggingface.co/jc-builds/smolvlm2-500m-gguf/resolve/main/smolvlm2-500m-Q8_0.gguf",
            ModelSizeBytes = 640L * 1024 * 1024,
            MinRamGb = 4,
            SupportsVision = true
        },
        new ModelTier
        {
            Id = "standard",
            Name = "Standard",
            Description = "Recommended. High-quality text chat, agent skills, and image Q&A in one model.",
            ModelFileName = "gemma-3-4b-it-Q4_K_M.gguf",
            MmProjFileName = "mmproj-gemma-3-4b-it-f16.gguf",
            DownloadUrl = "https://huggingface.co/ggml-org/gemma-3-4b-it-GGUF/resolve/main/gemma-3-4b-it-Q4_K_M.gguf",
            MmProjDownloadUrl = "https://huggingface.co/ggml-org/gemma-3-4b-it-GGUF/resolve/main/mmproj-gemma-3-4b-it-f16.gguf",
            ModelSizeBytes = 2500L * 1024 * 1024,
            MmProjSizeBytes = 500L * 1024 * 1024,
            MinRamGb = 6,
            SupportsVision = true
        },
        new ModelTier
        {
            Id = "pro",
            Name = "Pro",
            Description = "Best quality for complex reasoning and vision. Needs a flagship device.",
            ModelFileName = "phi4-mm-Q4_K_M.gguf",
            MmProjFileName = "mmproj-phi4-mm-f16.gguf",
            DownloadUrl = "https://huggingface.co/Swicked86/phi4-mm-gguf/resolve/main/phi4-mm-Q4_K_M.gguf",
            MmProjDownloadUrl = "https://huggingface.co/Swicked86/phi4-mm-gguf/resolve/main/mmproj-phi4-mm-f16.gguf",
            ModelSizeBytes = 2400L * 1024 * 1024,
            MmProjSizeBytes = 825L * 1024 * 1024,
            MinRamGb = 8,
            SupportsVision = true
        }
    };

    public static ModelTier? GetById(string id) =>
        AvailableTiers.FirstOrDefault(t => t.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
}
