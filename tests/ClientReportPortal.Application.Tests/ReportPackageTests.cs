using ClientReportPortal.Application.ReportPackages;
using ClientReportPortal.Domain.ReportPackages;
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

    // Submit section for review

    [Fact]
    public async Task Handle_MovesSectionToInReview()
    {
        var repository = new FakeReportPackageRepository();
        var package = ReportPackage.Create("Acme Wealth", "2026-Q3");
        var section = package.Sections.First(s => s.Type == SectionType.Performance);
        repository.Seed(package);

        var handler = new SubmitSectionForReviewHandler(repository);
        await handler.Handle(new SubmitSectionForReviewCommand(package.Id, section.Id), CancellationToken.None);

        repository.Saved!.Sections.Single(s => s.Id == section.Id).Status.Should().Be(SectionStatus.InReview);
    }

    [Fact]
    public async Task Handle_ThrowsNotFoundWhenPackageDoesNotExist()
    {
        var repository = new FakeReportPackageRepository();
        var handler = new SubmitSectionForReviewHandler(repository);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(new SubmitSectionForReviewCommand(Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None));
    }
}