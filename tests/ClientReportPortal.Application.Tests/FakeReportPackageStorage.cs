using ClientReportPortal.Application.Compilation;

namespace ClientReportPortal.Application.Tests;

public sealed class FakeReportPackageStorage : IReportPackageStorage
{
    public Task StoreAsync(Guid reportPackageId, CancellationToken ct) => Task.CompletedTask;
}
