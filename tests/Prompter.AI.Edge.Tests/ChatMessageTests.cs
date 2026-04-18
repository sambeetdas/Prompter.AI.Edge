using Xunit;
using Prompter.AI.Edge.Models;

namespace Prompter.AI.Edge.Tests;

public class ChatMessageTests
{
    [Fact]
    public void FromUser_SetsCorrectRole()
    {
        var msg = ChatMessage.FromUser("Hello");
        Assert.Equal(ChatRole.User, msg.Role);
        Assert.Equal("Hello", msg.Content);
    }

    [Fact]
    public void FromAssistant_SetsCorrectRole()
    {
        var msg = ChatMessage.FromAssistant("Hi there");
        Assert.Equal(ChatRole.Assistant, msg.Role);
    }

    [Fact]
    public void FromSystem_SetsCorrectRole()
    {
        var msg = ChatMessage.FromSystem("You are helpful");
        Assert.Equal(ChatRole.System, msg.Role);
    }

    [Fact]
    public void DownloadProgress_Idle_IsDefault()
    {
        var p = DownloadProgress.Idle;
        Assert.Equal(DownloadStatus.Idle, p.Status);
        Assert.Equal(0.0, p.Progress);
    }

    [Fact]
    public void DownloadProgress_Completed_HasFullProgress()
    {
        var p = DownloadProgress.Completed;
        Assert.Equal(DownloadStatus.Completed, p.Status);
        Assert.Equal(1.0, p.Progress);
    }

    [Fact]
    public void DownloadProgress_Error_HasMessage()
    {
        var p = DownloadProgress.Error("Network failed");
        Assert.Equal(DownloadStatus.Failed, p.Status);
        Assert.Equal("Network failed", p.ErrorMessage);
    }

    [Fact]
    public void ModelOptions_Default_HasSensibleValues()
    {
        var opts = ModelOptions.Default;
        Assert.Equal(2048, opts.ContextSize);
        Assert.Equal(512, opts.BatchSize);
        Assert.True(opts.Temperature > 0);
        Assert.True(opts.MaxTokens > 0);
    }
}
