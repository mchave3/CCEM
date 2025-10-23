using System.IO;
using System.Reflection;
using Velopack.Locators;

namespace CCEM.Core.Velopack.Tests.TestInfrastructure;

internal static class VelopackTestContext
{
    private static readonly object Sync = new();

    public static TestVelopackLocator ConfigureTestLocator(string version, string packagesDir)
    {
        Directory.CreateDirectory(packagesDir);

        var locator = new TestVelopackLocator(
            appId: "ccem.tests",
            version: version,
            packagesDir: packagesDir,
            logger: null);

        var setLocatorMethod = typeof(VelopackLocator).GetMethod(
            "SetCurrentLocator",
            BindingFlags.Static | BindingFlags.NonPublic)!;

        lock (Sync)
        {
            setLocatorMethod.Invoke(null, new object[] { locator });
        }

        return locator;
    }
}
