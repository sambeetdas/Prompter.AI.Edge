using System.Runtime.CompilerServices;
using Prompter.AI.Edge.Interfaces;
using Prompter.AI.Edge.Models;

namespace Prompter.AI.Edge.Services;

public sealed class SkillsEngine : ISkillsEngine
{
    private readonly IChatEngine _chatEngine;

    private static readonly IReadOnlyList<SkillDefinition> _builtInSkills = new[]
    {
        new SkillDefinition
        {
            Id = "summarizer", Name = "Summarizer",
            Description = "Condense long text into key points",
            Icon = "compress", Category = "Writing",
            SystemPrompt =
                "You are an expert summarizer. Read the provided text carefully and " +
                "produce a clear, concise summary that captures the main points and " +
                "key takeaways. Use bullet points for clarity. Keep the summary to " +
                "about 20-30% of the original length.",
            InputPlaceholder = "Paste the text you want to summarize..."
        },
        new SkillDefinition
        {
            Id = "translator", Name = "Translator",
            Description = "Translate text between languages",
            Icon = "translate", Category = "Language",
            SystemPrompt =
                "You are a professional translator. Translate the given text accurately " +
                "while preserving tone, context, and nuances. If the target language " +
                "is not specified, translate to English.",
            InputPlaceholder = "Enter text to translate...\n\nTip: Specify the target language, e.g. \"Translate to Spanish: Hello world\""
        },
        new SkillDefinition
        {
            Id = "code_explainer", Name = "Code Explainer",
            Description = "Explain code in simple terms",
            Icon = "code", Category = "Development",
            SystemPrompt =
                "You are a patient programming teacher. Explain the provided code in " +
                "simple, clear language that a beginner could understand. Break down " +
                "each important part, explain what it does and why.",
            InputPlaceholder = "Paste the code you want explained..."
        },
        new SkillDefinition
        {
            Id = "writing_assistant", Name = "Writing Assistant",
            Description = "Improve and polish your writing",
            Icon = "edit_note", Category = "Writing",
            SystemPrompt =
                "You are a professional writing assistant. Improve the provided text by " +
                "enhancing clarity, flow, grammar, and style while preserving the " +
                "original meaning and voice.",
            InputPlaceholder = "Paste your text to improve..."
        },
        new SkillDefinition
        {
            Id = "grammar_fixer", Name = "Grammar Fixer",
            Description = "Fix grammar and spelling errors",
            Icon = "spellcheck", Category = "Writing",
            SystemPrompt =
                "You are a grammar and spelling expert. Fix all grammatical errors, " +
                "spelling mistakes, and punctuation issues in the provided text. " +
                "Provide the corrected version, then list each correction.",
            InputPlaceholder = "Paste text with errors to fix..."
        },
        new SkillDefinition
        {
            Id = "brainstorm", Name = "Brainstorm Helper",
            Description = "Generate creative ideas on any topic",
            Icon = "lightbulb", Category = "Creative",
            SystemPrompt =
                "You are a creative brainstorming partner. Generate diverse, innovative " +
                "ideas related to the given topic. Provide at least 8-10 ideas ranging " +
                "from practical to creative.",
            InputPlaceholder = "What topic do you want to brainstorm about?"
        }
    };

    public SkillsEngine(IChatEngine chatEngine)
    {
        _chatEngine = chatEngine;
    }

    public IReadOnlyList<SkillDefinition> GetBuiltInSkills() => _builtInSkills;

    public async IAsyncEnumerable<string> ExecuteSkillAsync(
        string skillId, string input,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        var skill = _builtInSkills.FirstOrDefault(s =>
            s.Id.Equals(skillId, StringComparison.OrdinalIgnoreCase));

        if (skill is null)
        {
            yield return $"[Error: Unknown skill '{skillId}']";
            yield break;
        }

        var request = new ChatRequest
        {
            SystemPrompt = skill.SystemPrompt,
            Messages = new[] { ChatMessage.FromUser(input) }
        };

        await foreach (var token in _chatEngine.GenerateStreamAsync(request, ct))
        {
            yield return token;
        }
    }
}
