using System.Reflection;

namespace CCEM.Core.Sccm.Automation;

internal static class AutomationScripts
{
    private static readonly Assembly Assembly = typeof(AutomationScripts).Assembly;

    internal const string GetDcomPermTemplate =
        "$Reg = [WMIClass]\"root\\default:StdRegProv\"\n" +
        "$DCOM = $Reg.GetBinaryValue(2147483650,\"{0}\",\"{1}\").uValue\n" +
        "$security = Get-WmiObject -Namespace root/cimv2 -Class __SystemSecurity\n" +
        "$converter = new-object system.management.ManagementClass Win32_SecurityDescriptorHelper\n" +
        "$converter.BinarySDToSDDL($DCOM).SDDL\n";

    internal const string SetDcomPermTemplate =
        "$Reg = [WMIClass]\"root\\default:StdRegProv\"\n" +
        "$newDCOMSDDL = \"{2}\"\n" +
        "$DCOMbinarySD = $converter.SDDLToBinarySD($newDCOMSDDL)\n" +
        "$Reg.SetBinaryValue(2147483650,\"{0}\",\"{1}\", $DCOMbinarySD.binarySD)\n";

    internal static string CacheCleanup => Read("CacheCleanup.ps1");
    internal static string HealthCheck => Read("HealthCheck.ps1");
    internal static string SecretDecode => Read("SecretDecode.ps1");

    private static string Read(string fileName)
    {
        var resourceName = $"CCEM.Core.Sccm.Automation.Scripts.{fileName}";
        using var stream = Assembly.GetManifestResourceStream(resourceName);
        if (stream is null)
        {
            throw new InvalidOperationException($"Embedded resource not found: {resourceName}");
        }

        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
