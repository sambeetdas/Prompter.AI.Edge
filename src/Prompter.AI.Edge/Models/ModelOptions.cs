namespace Prompter.AI.Edge.Models;

public sealed class ModelOptions
{
    public int ContextSize { get; init; } = 2048;
    public int BatchSize { get; init; } = 512;
    public int GpuLayers { get; init; } = -1;
    public int ThreadCount { get; init; } = 4;
    public float Temperature { get; init; } = 0.7f;
    public float TopP { get; init; } = 0.9f;
    public int MaxTokens { get; init; } = 1024;
    public float RepeatPenalty { get; init; } = 1.1f;

    public static ModelOptions Default => new();
}
