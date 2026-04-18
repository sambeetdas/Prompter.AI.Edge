using Prompter.AI.Edge.Interfaces;
using Prompter.AI.Edge.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Prompter.AI.Edge.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers all Prompter.AI.Edge services into the DI container.
    /// The consumer must provide the <paramref name="modelsDirectory"/> where
    /// GGUF model files will be stored on disk.
    /// </summary>
    public static IServiceCollection AddPrompterAIEdge(this IServiceCollection services, string modelsDirectory)
    {
        services.AddHttpClient<IModelManager, ModelManager>(client =>
        {
            client.Timeout = TimeSpan.FromHours(2);
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Prompter.AI.Edge/1.0");
        })
        .ConfigureHttpClient(_ => { })
        .AddTypedClient<IModelManager>((httpClient, sp) =>
        {
            var logger = sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<ModelManager>>();
            return new ModelManager(httpClient, logger, modelsDirectory);
        });

        services.AddSingleton<IChatEngine, ChatEngine>();
        services.AddSingleton<ISkillsEngine, SkillsEngine>();
        services.AddSingleton<IVisionEngine, VisionEngine>();

        return services;
    }
}
