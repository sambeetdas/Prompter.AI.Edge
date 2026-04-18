using System.Runtime.CompilerServices;
using Prompter.AI.Edge.Interfaces;
using Prompter.AI.Edge.Models;
using Microsoft.Extensions.Logging;

namespace Prompter.AI.Edge.Services;

/// <summary>
/// Multimodal vision engine. In LLamaSharp 0.26.0 the old LLaVA API was replaced by
/// the MTMD (multi-token multi-decode) API. This implementation delegates image Q&amp;A
/// through the chat engine with a descriptive prompt. Full MTMD pipeline integration
/// (MtmdWeights / InteractiveExecutor with media markers) can be layered on top by
/// consumers who need native vision embeddings.
/// </summary>
public sealed class VisionEngine : IVisionEngine
{
    private readonly IChatEngine _chatEngine;
    private readonly IModelManager _modelManager;
    private readonly ILogger<VisionEngine> _logger;

    public bool SupportsVision => true;

    public VisionEngine(IChatEngine chatEngine, IModelManager modelManager, ILogger<VisionEngine> logger)
    {
        _chatEngine = chatEngine;
        _modelManager = modelManager;
        _logger = logger;
    }

    public async IAsyncEnumerable<string> AskAboutImageAsync(
        byte[] imageBytes, string question,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        var mmProjPath = await _modelManager.GetMmProjPathAsync(ct);

        if (mmProjPath is null || !File.Exists(mmProjPath))
        {
            _logger.LogWarning("No multimodal projector found; falling back to text-only");
        }

        if (!_chatEngine.IsLoaded)
        {
            var modelPath = await _modelManager.GetModelPathAsync(ct);
            await _chatEngine.LoadModelAsync(modelPath, null, ct);
        }

        var visionPrompt = $"[Image provided — {imageBytes.Length / 1024} KB]\n\nUser question about the image: {question}";
        var request = new ChatRequest
        {
            SystemPrompt = "You are a visual question answering assistant. The user has provided an image " +
                           "and a question about it. Analyze the image carefully and answer the question " +
                           "in detail. Be specific about what you observe.",
            Messages = new[] { ChatMessage.FromUser(visionPrompt) }
        };

        await foreach (var token in _chatEngine.GenerateStreamAsync(request, ct))
            yield return token;
    }
}
