using Microsoft.Win32;

namespace CCEM.Services;

public sealed record SccmConsoleInfo(string UiInstallationDirectory, string? ConnectedServer);

public static class SccmConsoleLocator
{
    public static SccmConsoleInfo? TryGetConsoleInfo()
    {
        var uiPath = TryGetUiInstallDirectory();
        if (string.IsNullOrWhiteSpace(uiPath))
        {
            return null;
        }

        var server = TryGetAdminUiServer();
        return new SccmConsoleInfo(uiPath, server);
    }

    private static string? TryGetUiInstallDirectory()
    {
        var setupKeyPath = Environment.Is64BitOperatingSystem
            ? @"SOFTWARE\Wow6432Node\Microsoft\ConfigMgr10\Setup"
            : @"SOFTWARE\Microsoft\ConfigMgr10\Setup";

        try
        {
            using var setupKey = Registry.LocalMachine.OpenSubKey(setupKeyPath);
            var value = setupKey?.GetValue("UI Installation Directory", "")?.ToString();
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            return Directory.Exists(value) ? value : null;
        }
        catch
        {
            return null;
        }
    }

    private static string? TryGetAdminUiServer()
    {
        var connectionKeyPath = Environment.Is64BitOperatingSystem
            ? @"SOFTWARE\Wow6432Node\Microsoft\ConfigMgr10\AdminUI\Connection"
            : @"SOFTWARE\Microsoft\ConfigMgr10\AdminUI\Connection";

        try
        {
            using var connectionKey = Registry.LocalMachine.OpenSubKey(connectionKeyPath);
            var value = connectionKey?.GetValue("Server", "")?.ToString();
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            return value;
        }
        catch
        {
            return null;
        }
    }
}

