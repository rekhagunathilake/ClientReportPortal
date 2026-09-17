using ClientReportPortal.Domain.ReportPackages;

namespace ClientReportPortal.Domain.Tests;

public class ReportPackageTests
{
    [Fact]
    public void Create_SeedsThreeSectionsAsPending_AndStartsInDraft()
    {
        var package = ReportPackage.Create("Acme Wealth", "2026-Q3");

        Assert.Equal(ReportPackageStatus.Draft, package.Status);
        Assert.Equal(3, package.Sections.Count);
        Assert.All(package.Sections, s => Assert.Equal(SectionStatus.Pending, s.Status));
        Assert.Contains(package.Sections, s => s.Type == SectionType.Performance);
        Assert.Contains(package.Sections, s => s.Type == SectionType.Holdings);
        Assert.Contains(package.Sections, s => s.Type == SectionType.Commentary);
    }

    [Fact]
    public void SubmitSectionForReview_MovesSectionFromPendingToInReview()
    {
        var package = ReportPackage.Create("Acme Wealth", "2026-Q3");
        var section = package.Sections.First(s => s.Type == SectionType.Performance);

        package.SubmitSectionForReview(section.Id);

        var updated = package.Sections.Single(s => s.Id == section.Id);
        Assert.Equal(SectionStatus.InReview, updated.Status);
    }

    [Fact]
    public void ApproveSection_MovesSectionFromInReviewToApproved()
    {
        var package = ReportPackage.Create("Acme Wealth", "2026-Q3");
        var section = package.Sections.First(s => s.Type == SectionType.Performance);
        package.SubmitSectionForReview(section.Id);

        package.ApproveSection(section.Id);

        var updated = package.Sections.Single(s => s.Id == section.Id);
        Assert.Equal(SectionStatus.Approved, updated.Status);
    }

    [Fact]
    public void ApproveSection_ThrowsWhenSectionIsNotInReview()
    {
        var package = ReportPackage.Create("Acme Wealth", "2026-Q3");
        var section = package.Sections.First(s => s.Type == SectionType.Performance);
        // still Pending — never submitted for review

        Assert.Throws<DomainInvariantViolationException>(() => package.ApproveSection(section.Id));

        var unchanged = package.Sections.Single(s => s.Id == section.Id);
        Assert.Equal(SectionStatus.Pending, unchanged.Status);
    }

    [Fact]
    public void RejectSection_MovesSectionFromInReviewToRejected()
    {
        var package = ReportPackage.Create("Acme Wealth", "2026-Q3");
        var section = package.Sections.First(s => s.Type == SectionType.Performance);
        package.SubmitSectionForReview(section.Id);

        package.RejectSection(section.Id);

        var updated = package.Sections.Single(s => s.Id == section.Id);
        Assert.Equal(SectionStatus.Rejected, updated.Status);
    }

    [Fact]
    public void ReviseSection_MovesSectionFromRejectedToPending()
    {
        var package = ReportPackage.Create("Acme Wealth", "2026-Q3");
        var section = package.Sections.First(s => s.Type == SectionType.Performance);
        package.SubmitSectionForReview(section.Id);
        package.RejectSection(section.Id);

        package.ReviseSection(section.Id);

        var updated = package.Sections.Single(s => s.Id == section.Id);
        Assert.Equal(SectionStatus.Pending, updated.Status);
    }
}