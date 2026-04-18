namespace Prompter.AI.Edge.Models;

public enum ChatRole
{
    System,
    User,
    Assistant
}

public sealed class ChatMessage
{
    public required ChatRole Role { get; init; }
    public required string Content { get; init; }

    public static ChatMessage FromSystem(string content) =>
        new() { Role = ChatRole.System, Content = content };

    public static ChatMessage FromUser(string content) =>
        new() { Role = ChatRole.User, Content = content };

    public static ChatMessage FromAssistant(string content) =>
        new() { Role = ChatRole.Assistant, Content = content };
}
