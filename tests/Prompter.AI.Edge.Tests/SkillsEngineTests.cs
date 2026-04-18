using Xunit;
using Prompter.AI.Edge.Interfaces;
using Prompter.AI.Edge.Services;
using Moq;

namespace Prompter.AI.Edge.Tests;

public class SkillsEngineTests
{
    [Fact]
    public void GetBuiltInSkills_ReturnsSixSkills()
    {
        var chatEngine = new Mock<IChatEngine>();
        var engine = new SkillsEngine(chatEngine.Object);

        var skills = engine.GetBuiltInSkills();

        Assert.Equal(6, skills.Count);
    }

    [Fact]
    public void GetBuiltInSkills_ContainsExpectedIds()
    {
        var chatEngine = new Mock<IChatEngine>();
        var engine = new SkillsEngine(chatEngine.Object);

        var skills = engine.GetBuiltInSkills();
        var ids = skills.Select(s => s.Id).ToList();

        Assert.Contains("summarizer", ids);
        Assert.Contains("translator", ids);
        Assert.Contains("code_explainer", ids);
        Assert.Contains("writing_assistant", ids);
        Assert.Contains("grammar_fixer", ids);
        Assert.Contains("brainstorm", ids);
    }

    [Fact]
    public void AllSkills_HaveNonEmptySystemPrompt()
    {
        var chatEngine = new Mock<IChatEngine>();
        var engine = new SkillsEngine(chatEngine.Object);

        foreach (var skill in engine.GetBuiltInSkills())
        {
            Assert.False(string.IsNullOrWhiteSpace(skill.SystemPrompt),
                $"Skill '{skill.Id}' has empty system prompt");
            Assert.False(string.IsNullOrWhiteSpace(skill.InputPlaceholder),
                $"Skill '{skill.Id}' has empty input placeholder");
        }
    }
}
