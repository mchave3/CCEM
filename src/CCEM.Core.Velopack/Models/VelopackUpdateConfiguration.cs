namespace CCEM.Core.Velopack.Models;

/// <summary>
/// Defines the configuration used to locate and authenticate against a Velopack update feed.
/// </summary>
public sealed class VelopackUpdateConfiguration
{
    /// <summary>
    /// Initializes the configuration with the GitHub repository that hosts Velopack packages.
    /// </summary>
    /// <param name="repositoryUrl">The HTTPS URL to the Velopack-enabled repository.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="repositoryUrl"/> is null or whitespace.</exception>
    public VelopackUpdateConfiguration(string repositoryUrl)
    {
        if (string.IsNullOrWhiteSpace(repositoryUrl))
        {
            throw new ArgumentException("A valid GitHub repository URL is required.", nameof(repositoryUrl));
        }

        RepositoryUrl = repositoryUrl;
    }

    /// <summary>
    /// Gets the GitHub repository URL used as the update source.
    /// </summary>
    public string RepositoryUrl { get; }

    /// <summary>
    /// Gets or sets the personal access token used when accessing private repositories.
    /// </summary>
    public string? AccessToken { get; init; }
}
