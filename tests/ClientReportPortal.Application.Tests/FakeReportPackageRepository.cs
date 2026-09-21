using ClientReportPortal.Application.ReportPackages;
using ClientReportPortal.Domain.ReportPackages;

namespace ClientReportPortal.Application.Tests;

public sealed class FakeReportPackageRepository : IReportPackageRepository
{
    private readonly Dictionary<Guid, ReportPackage> _packages = new();

    public ReportPackage? Added { get; private set; }
    public ReportPackage? Saved { get; private set; }

    public Task AddAsync(ReportPackage package, CancellationToken ct)
    {
        Added = package;
        _packages[package.Id] = package;
        return Task.CompletedTask;
    }

    public Task<ReportPackage?> GetAsync(Guid id, CancellationToken ct) =>
        Task.FromResult(_packages.GetValueOrDefault(id));

    public Task SaveAsync(ReportPackage package, CancellationToken ct)
    {
        Saved = package;
        _packages[package.Id] = package;
        return Task.CompletedTask;
    }

    public void Seed(ReportPackage package) => _packages[package.Id] = package;
}