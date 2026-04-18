namespace Prompter.AI.Edge.Interfaces;

public interface IVisionEngine
{
    IAsyncEnumerable<string> AskAboutImageAsync(byte[] imageBytes, string question, CancellationToken ct = default);
    bool SupportsVision { get; }
}
