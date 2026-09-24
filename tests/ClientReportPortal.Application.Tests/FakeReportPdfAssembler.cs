using ClientReportPortal.Application.Compilation;

namespace ClientReportPortal.Application.Tests;

public sealed class FakeReportPdfAssembler : IReportPdfAssembler
{
    public Task AssembleAsync(Guid reportPackageId, CancellationToken ct) => Task.CompletedTask;
}
