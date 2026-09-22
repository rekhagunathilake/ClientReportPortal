using ClientReportPortal.Application.Compilation;

namespace ClientReportPortal.Application.Tests;

public sealed class FakeSectionRenderer : ISectionRenderer
{
    public Task RenderAsync(Guid reportPackageId, CancellationToken ct) => Task.CompletedTask;
}