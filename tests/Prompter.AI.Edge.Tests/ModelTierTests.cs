using Xunit;
using Prompter.AI.Edge.Models;

namespace Prompter.AI.Edge.Tests;

public class ModelTierTests
{
    [Fact]
    public void AvailableTiers_ContainsThreeTiers()
    {
        Assert.Equal(3, ModelTier.AvailableTiers.Count);
    }

    [Theory]
    [InlineData("lite")]
    [InlineData("standard")]
    [InlineData("pro")]
    public void GetById_ReturnsCorrectTier(string id)
    {
        var tier = ModelTier.GetById(id);
        Assert.NotNull(tier);
        Assert.Equal(id, tier!.Id);
    }

    [Fact]
    public void GetById_ReturnsNull_ForUnknownId()
    {
        var tier = ModelTier.GetById("nonexistent");
        Assert.Null(tier);
    }

    [Fact]
    public void TotalSizeBytes_IncludesMmProj()
    {
        var tier = ModelTier.GetById("standard")!;
        Assert.True(tier.TotalSizeBytes > tier.ModelSizeBytes);
        Assert.Equal(tier.ModelSizeBytes + tier.MmProjSizeBytes!.Value, tier.TotalSizeBytes);
    }

    [Fact]
    public void TotalSizeFormatted_ReturnsReadableString()
    {
        var tier = ModelTier.GetById("lite")!;
        Assert.Contains("MB", tier.TotalSizeFormatted);
    }

    [Fact]
    public void AllTiers_SupportVision()
    {
        foreach (var tier in ModelTier.AvailableTiers)
            Assert.True(tier.SupportsVision);
    }

    [Fact]
    public void AllTiers_HaveValidDownloadUrls()
    {
        foreach (var tier in ModelTier.AvailableTiers)
        {
            Assert.StartsWith("https://", tier.DownloadUrl);
            Assert.EndsWith(".gguf", tier.DownloadUrl);
        }
    }
}
