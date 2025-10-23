using Velopack;

namespace CCEM.Core.Velopack.Bootstrap;

/// <summary>
/// Provides a simple entry point for initializing Velopack in bootstrap scenarios.
/// </summary>
public static class VelopackBootstrapper
{
    /// <summary>
    /// Configures and starts the Velopack bootstrapper using default settings.
    /// </summary>
    public static void Initialize()
    {
        VelopackApp.Build().Run();
    }
}
