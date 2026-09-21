using ClientReportPortal.Application.ReportPackages;
using FluentAssertions;

namespace ClientReportPortal.Application.Tests;
public class ReportPackageTests
{
    [Fact]
    public async Task Handle_CreatesPackageAndPersistsIt()
    {
        var repository = new FakeReportPackageRepository();
        var handler = new CreateReportPackageHandler(repository);

        var id = await handler.Handle(new CreateReportPackageCommand("Acme Wealth", "2026-Q3"), CancellationToken.None);

        repository.Added.Should().NotBeNull();
        repository.Added!.Id.Should().Be(id);
        repository.Added.ClientName.Should().Be("Acme Wealth");
        repository.Added.Quarter.Should().Be("2026-Q3");
    }
}