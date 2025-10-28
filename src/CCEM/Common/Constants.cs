namespace CCEM.Common;

public static partial class Constants
{
    // ProgramData directory structure (main application configuration)
    public static readonly string RootDirectoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "CCEM");
    public static readonly string LogDirectoryPath = Path.Combine(RootDirectoryPath, "Logs");
    public static readonly string LogFilePath = Path.Combine(LogDirectoryPath, "CCEM.log");
    public static readonly string AppConfigPath = Path.Combine(RootDirectoryPath, "AppConfig.json");
}
