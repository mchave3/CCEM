using CCEM.Core.Remote;

namespace CCEM.Core.Sccm.Tests;

public class WinRmPowerShellClientTests
{
    [Fact]
    public async Task InvokeAsync_EmptyScript_Throws()
    {
        var client = new WinRmPowerShellClient();
        await Assert.ThrowsAsync<ArgumentException>(() => client.InvokeAsync(new PowerShellInvocationRequest { Script = string.Empty }));
    }
}
