namespace ClientReportPortal.Application.Compilation;

public interface IReportPdfAssembler
{
    Task AssembleAsync(Guid reportPackageId, CancellationToken ct);
}