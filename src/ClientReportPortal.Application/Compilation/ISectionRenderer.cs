namespace ClientReportPortal.Application.Compilation;

public interface ISectionRenderer
{
    Task RenderAsync(Guid reportPackageId, CancellationToken ct);
}