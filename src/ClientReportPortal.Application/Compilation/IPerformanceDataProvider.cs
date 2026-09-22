namespace ClientReportPortal.Application.Compilation;

public interface IPerformanceDataProvider
{
    Task GetAsync(Guid reportPackageId, CancellationToken ct);
}
