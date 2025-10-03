using System.Diagnostics;

namespace CCEM.Shared.Helpers;

/// <summary>
/// Helper class for executing commands and processes
/// </summary>
public static class CommandHelper
{
    /// <summary>
    /// Executes a command asynchronously
    /// </summary>
    /// <param name="fileName">The executable file name</param>
    /// <param name="arguments">Command line arguments</param>
    /// <param name="workingDirectory">Working directory for the command</param>
    /// <returns>Command output and exit code</returns>
    public static async Task<(string Output, int ExitCode)> ExecuteCommandAsync(
        string fileName,
        string? arguments = null,
        string? workingDirectory = null)
    {
        try
        {
            var processStartInfo = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments ?? string.Empty,
                WorkingDirectory = workingDirectory ?? Environment.CurrentDirectory,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = new Process { StartInfo = processStartInfo };
            process.Start();

            var outputTask = process.StandardOutput.ReadToEndAsync();
            var errorTask = process.StandardError.ReadToEndAsync();

            await process.WaitForExitAsync();

            var output = await outputTask;
            var error = await errorTask;

            var combinedOutput = string.IsNullOrEmpty(error)
                ? output
                : $"{output}\n{error}";

            return (combinedOutput, process.ExitCode);
        }
        catch (Exception ex)
        {
            return ($"Error executing command: {ex.Message}", -1);
        }
    }

    /// <summary>
    /// Executes a PowerShell command asynchronously
    /// </summary>
    /// <param name="script">PowerShell script to execute</param>
    /// <param name="workingDirectory">Working directory for the script</param>
    /// <returns>Script output and exit code</returns>
    public static async Task<(string Output, int ExitCode)> ExecutePowerShellAsync(
        string script,
        string? workingDirectory = null)
    {
        var encodedScript = Convert.ToBase64String(
            System.Text.Encoding.Unicode.GetBytes(script));

        return await ExecuteCommandAsync(
            "powershell.exe",
            $"-NoProfile -NonInteractive -EncodedCommand {encodedScript}",
            workingDirectory);
    }

    /// <summary>
    /// Runs an elevated command (requires UAC prompt)
    /// </summary>
    /// <param name="fileName">The executable file name</param>
    /// <param name="arguments">Command line arguments</param>
    /// <returns>True if command was started successfully</returns>
    public static bool RunElevated(string fileName, string? arguments = null)
    {
        try
        {
            var processStartInfo = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments ?? string.Empty,
                UseShellExecute = true,
                Verb = "runas"
            };

            using var process = Process.Start(processStartInfo);
            return process != null;
        }
        catch
        {
            return false;
        }
    }
}
