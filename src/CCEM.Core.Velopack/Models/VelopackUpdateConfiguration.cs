namespace CCEM.Core.Velopack.Models;

public sealed class VelopackUpdateConfiguration
{
    public VelopackUpdateConfiguration(string repositoryUrl)
    {
        if (string.IsNullOrWhiteSpace(repositoryUrl))
        {
            throw new ArgumentException("A valid GitHub repository URL is required.", nameof(repositoryUrl));
        }

        RepositoryUrl = repositoryUrl;
    }

    public string RepositoryUrl { get; }
    public string? AccessToken { get; init; }
}
