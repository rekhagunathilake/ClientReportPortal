namespace ClientReportPortal.Application.Compilation;

public interface IReportPackageStorage
{
    Task StoreAsync(Guid reportPackageId, CancellationToken ct);
}