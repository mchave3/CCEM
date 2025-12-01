using CCEM.Core.Sccm.Models;
using CCEM.Core.Sccm.Services;

namespace CCEM.Core.Sccm.Tests;

public class WmiQueryServiceTests
{
    [Fact]
    public async Task QueryAsync_InvalidScope_Throws()
    {
        var sut = new WmiQueryService();
        await Assert.ThrowsAsync<ArgumentException>(() => sut.QueryAsync(new WmiQuery("", "SELECT * FROM Win32_ComputerSystem")));
    }

    [Fact(Skip = "Requires local WMI endpoint; run manually when environment allows.")]
    public async Task QueryAsync_Localhost_Win32ComputerSystem()
    {
        var sut = new WmiQueryService();
        var result = await sut.QueryAsync(new WmiQuery(@"\\\\.\\root\\cimv2", "SELECT Name FROM Win32_ComputerSystem"));
        Assert.NotNull(result);
    }
}
