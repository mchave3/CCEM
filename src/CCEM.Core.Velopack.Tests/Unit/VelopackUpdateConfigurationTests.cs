using CCEM.Core.Velopack.Models;

namespace CCEM.Core.Velopack.Tests.Unit;

public sealed class VelopackUpdateConfigurationTests
{
    [Fact]
    public void Constructor_Throws_WhenRepositoryUrlIsNullOrWhitespace()
    {
        Assert.Throws<ArgumentException>(() => new VelopackUpdateConfiguration(" "));
    }

    [Fact]
    public void Constructor_SetsRepositoryUrl()
    {
        const string repositoryUrl = "https://github.com/example/repo";
        var configuration = new VelopackUpdateConfiguration(repositoryUrl);

        Assert.Equal(repositoryUrl, configuration.RepositoryUrl);
    }
}
