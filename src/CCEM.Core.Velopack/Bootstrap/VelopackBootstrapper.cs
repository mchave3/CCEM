using Velopack;

namespace CCEM.Core.Velopack.Bootstrap;

public static class VelopackBootstrapper
{
    public static void Initialize()
    {
        VelopackApp.Build().Run();
    }
}
