using System.Threading;
using System.Threading.Tasks;

namespace CCEM.Core.Startup;

/// <summary>
/// Describes a UI surface capable of showing a splash experience during application startup.
/// </summary>
public interface ISplashScreenHost
{
    /// <summary>
    /// Ensures the splash content is visible. Safe to call multiple times.
    /// </summary>
    void ShowSplash();

    /// <summary>
    /// Updates the textual status displayed by the splash experience.
    /// </summary>
    /// <param name="message">The status message to render.</param>
    void UpdateStatus(string message);

    /// <summary>
    /// Transitions from the splash presentation into the main application shell.
    /// </summary>
    /// <param name="cancellationToken">Propagation token.</param>
    Task EnterShellAsync(CancellationToken cancellationToken = default);
}
