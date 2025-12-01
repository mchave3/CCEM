using CCEM.Core.Sccm.Models;

namespace CCEM.Core.Sccm.Contracts;

public interface ILogCaptureService
{
    IAsyncEnumerable<string> TailAsync(SccmConnectionInfo connection, string logPath, int maxLines = 200, CancellationToken cancellationToken = default);
}
