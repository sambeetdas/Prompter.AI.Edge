using Prompter.AI.Edge.Models;

namespace Prompter.AI.Edge.Interfaces;

public interface ISkillsEngine
{
    IReadOnlyList<SkillDefinition> GetBuiltInSkills();
    IAsyncEnumerable<string> ExecuteSkillAsync(string skillId, string input, CancellationToken ct = default);
}
