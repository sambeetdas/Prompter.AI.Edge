using System.Runtime.CompilerServices;
using System.Text;
using Prompter.AI.Edge.Interfaces;
using Prompter.AI.Edge.Models;
using LLama;
using LLama.Common;
using LLama.Sampling;
using Microsoft.Extensions.Logging;

namespace Prompter.AI.Edge.Services;

public sealed class ChatEngine : IChatEngine, IDisposable
{
    private readonly ILogger<ChatEngine> _logger;
    private LLamaWeights? _model;
    private LLamaContext? _context;
    private ModelOptions _options = ModelOptions.Default;
    private CancellationTokenSource? _generationCts;
    private readonly SemaphoreSlim _lock = new(1, 1);

    public bool IsLoaded => _model is not null && _context is not null;

    public ChatEngine(ILogger<ChatEngine> logger)
    {
        _logger = logger;
    }

    public async Task LoadModelAsync(string modelPath, ModelOptions? options = null, CancellationToken ct = default)
    {
        await _lock.WaitAsync(ct);
        try
        {
            if (IsLoaded) await UnloadModelInternalAsync();

            _options = options ?? ModelOptions.Default;

            var modelParams = new ModelParams(modelPath)
            {
                ContextSize = (uint)_options.ContextSize,
                BatchSize = (uint)_options.BatchSize,
                GpuLayerCount = _options.GpuLayers,
                Threads = _options.ThreadCount,
            };

            _logger.LogInformation("Loading model from {Path}", modelPath);
            _model = await LLamaWeights.LoadFromFileAsync(modelParams, ct);
            _context = _model.CreateContext(modelParams);
            _logger.LogInformation("Model loaded successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load model from {Path}", modelPath);
            await UnloadModelInternalAsync();
            throw;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async IAsyncEnumerable<string> GenerateStreamAsync(
        ChatRequest request,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        if (!IsLoaded)
            throw new InvalidOperationException("Model not loaded. Call LoadModelAsync first.");

        _generationCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        var linkedCt = _generationCts.Token;

        await _lock.WaitAsync(linkedCt);
        try
        {
            var executor = new StatelessExecutor(_model!, _context!.Params);
            var prompt = BuildPrompt(request);

            var inferenceParams = new InferenceParams
            {
                MaxTokens = request.MaxTokens ?? _options.MaxTokens,
                AntiPrompts = new[] { "<|end|>", "<|user|>", "</s>" },
                SamplingPipeline = new DefaultSamplingPipeline
                {
                    Temperature = request.Temperature ?? _options.Temperature,
                    TopP = request.TopP ?? _options.TopP,
                    RepeatPenalty = _options.RepeatPenalty,
                }
            };

            await foreach (var token in executor.InferAsync(prompt, inferenceParams, linkedCt))
            {
                yield return token;
            }
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<string> GenerateAsync(ChatRequest request, CancellationToken ct = default)
    {
        var sb = new StringBuilder();
        await foreach (var token in GenerateStreamAsync(request, ct))
        {
            sb.Append(token);
        }
        return sb.ToString();
    }

    public void StopGeneration()
    {
        _generationCts?.Cancel();
        _generationCts = null;
    }

    public async Task UnloadModelAsync()
    {
        await _lock.WaitAsync();
        try
        {
            await UnloadModelInternalAsync();
        }
        finally
        {
            _lock.Release();
        }
    }

    private Task UnloadModelInternalAsync()
    {
        _context?.Dispose();
        _context = null;
        _model?.Dispose();
        _model = null;
        _logger.LogInformation("Model unloaded");
        return Task.CompletedTask;
    }

    private static string BuildPrompt(ChatRequest request)
    {
        var sb = new StringBuilder();

        var systemPrompt = request.SystemPrompt ?? ChatRequest.DefaultSystemPrompt;
        sb.AppendLine($"<|system|>\n{systemPrompt}\n<|end|>");

        foreach (var msg in request.Messages)
        {
            var role = msg.Role switch
            {
                ChatRole.User => "user",
                ChatRole.Assistant => "assistant",
                ChatRole.System => "system",
                _ => "user"
            };
            sb.AppendLine($"<|{role}|>\n{msg.Content}\n<|end|>");
        }

        sb.AppendLine("<|assistant|>");
        return sb.ToString();
    }

    public void Dispose()
    {
        _generationCts?.Cancel();
        _generationCts?.Dispose();
        _context?.Dispose();
        _model?.Dispose();
        _lock.Dispose();
    }
}
