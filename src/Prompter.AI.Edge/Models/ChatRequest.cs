namespace Prompter.AI.Edge.Models;

public sealed class ChatRequest
{
    public required IReadOnlyList<ChatMessage> Messages { get; init; }
    public string? SystemPrompt { get; init; }
    public float? Temperature { get; init; }
    public float? TopP { get; init; }
    public int? MaxTokens { get; init; }

    public static readonly string DefaultSystemPrompt =
        "You are a helpful, friendly AI assistant running locally on this device. " +
        "You provide clear, concise answers.";
}
