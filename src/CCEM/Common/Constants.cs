namespace CCEM.Common;

public static partial class Constants
{
    public static readonly string RootDirectoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "CCEM");
    public static readonly string LogDirectoryPath = Path.Combine(RootDirectoryPath, "Log");
    public static readonly string LogFilePath = Path.Combine(LogDirectoryPath, "CCEM.log");
    public static readonly string AppConfigPath = Path.Combine(RootDirectoryPath, "AppConfig.json");
}
