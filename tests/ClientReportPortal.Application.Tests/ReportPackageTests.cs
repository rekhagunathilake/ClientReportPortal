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

    // Approve section 

    [Fact]
    public async Task ApproveSection_MovesSectionToApproved()
    {
        var repository = new FakeReportPackageRepository();
        var package = ReportPackage.Create("Acme Wealth", "2026-Q3");
        var section = package.Sections.First(s => s.Type == SectionType.Performance);
        package.SubmitSectionForReview(section.Id);
        repository.Seed(package);

        var handler = new ApproveSectionHandler(repository);
        await handler.Handle(new ApproveSectionCommand(package.Id, section.Id), CancellationToken.None);

        repository.Saved!.Sections.Single(s => s.Id == section.Id).Status.Should().Be(SectionStatus.Approved);
    }

    [Fact]
    public async Task ApproveSection_ThrowsNotFoundWhenPackageDoesNotExist()
    {
        var repository = new FakeReportPackageRepository();
        var handler = new ApproveSectionHandler(repository);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(new ApproveSectionCommand(Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None));
    }

    // Reject section

    [Fact]
    public async Task RejectSection_MovesSectionToRejected()
    {
        var repository = new FakeReportPackageRepository();
        var package = ReportPackage.Create("Acme Wealth", "2026-Q3");
        var section = package.Sections.First(s => s.Type == SectionType.Performance);
        package.SubmitSectionForReview(section.Id);
        repository.Seed(package);

        var handler = new RejectSectionHandler(repository);
        await handler.Handle(new RejectSectionCommand(package.Id, section.Id), CancellationToken.None);

        repository.Saved!.Sections.Single(s => s.Id == section.Id).Status.Should().Be(SectionStatus.Rejected);
    }

    [Fact]
    public async Task RejectSection_ThrowsNotFoundWhenPackageDoesNotExist()
    {
        var repository = new FakeReportPackageRepository();
        var handler = new RejectSectionHandler(repository);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(new RejectSectionCommand(Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None));
    }
}