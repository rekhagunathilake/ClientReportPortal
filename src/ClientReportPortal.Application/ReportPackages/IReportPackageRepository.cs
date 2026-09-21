using ClientReportPortal.Domain.ReportPackages;

namespace ClientReportPortal.Application.ReportPackages;
public interface IReportPackageRepository
{
    Task AddAsync(ReportPackage package, CancellationToken ct);

    Task<ReportPackage?> GetAsync(Guid id, CancellationToken ct);

    Task SaveAsync(ReportPackage package, CancellationToken ct);
}
