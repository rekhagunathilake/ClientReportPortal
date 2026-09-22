using ClientReportPortal.Application.Compilation;

namespace ClientReportPortal.Application.Tests;

public sealed class FakePerformanceDataProvider : IPerformanceDataProvider
{
    public Task GetAsync(Guid reportPackageId, CancellationToken ct) => Task.CompletedTask;
}
