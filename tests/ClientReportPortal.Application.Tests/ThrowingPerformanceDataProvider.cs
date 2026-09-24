using ClientReportPortal.Application.Compilation;

namespace ClientReportPortal.Application.Tests;

public sealed class ThrowingPerformanceDataProvider : IPerformanceDataProvider
{
    public Task GetAsync(Guid reportPackageId, CancellationToken ct) =>
        throw new InvalidOperationException("Simulated failure.");
}
