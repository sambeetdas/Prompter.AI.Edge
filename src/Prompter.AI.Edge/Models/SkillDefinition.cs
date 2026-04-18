namespace Prompter.AI.Edge.Models;

public sealed class SkillDefinition
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required string Description { get; init; }
    public required string Icon { get; init; }
    public required string SystemPrompt { get; init; }
    public required string InputPlaceholder { get; init; }
    public required string Category { get; init; }
    public bool IsBuiltIn { get; init; } = true;
}
