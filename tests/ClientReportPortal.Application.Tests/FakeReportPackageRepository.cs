using ClientReportPortal.Application.ReportPackages;
using ClientReportPortal.Domain.ReportPackages;

namespace ClientReportPortal.Application.Tests;

public sealed class FakeReportPackageRepository : IReportPackageRepository
{
    public ReportPackage? Added { get; private set; }

    public Task AddAsync(ReportPackage package, CancellationToken ct)
    {
        Added = package;
        return Task.CompletedTask;
    }
}