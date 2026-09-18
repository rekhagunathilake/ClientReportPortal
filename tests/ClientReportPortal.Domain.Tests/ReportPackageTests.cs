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
    public void SubmitSectionForReview_ThrowsWhenSectionIsNotPending()
    {
        var package = ReportPackage.Create("Acme Wealth", "2026-Q3");
        var section = package.Sections.First(s => s.Type == SectionType.Performance);

        package.SubmitSectionForReview(section.Id);

        Assert.Throws<DomainInvariantViolationException>(() => package.SubmitSectionForReview(section.Id)); // calling SubmitSectionForReview() twice in a row
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

    [Fact]
    public void StartCompilation_ThrowsWhenNotAllSectionsApproved()
    {
        var package = ReportPackage.Create("Acme Wealth", "2026-Q3");
        // all three sections still Pending — none approved

        Assert.Throws<DomainInvariantViolationException>(package.StartCompilation);
    }

    [Fact]
    public void StartCompilation_MovesToCompiling_WhenAllSectionsApproved()
    {
        var package = ReportPackage.Create("Acme Wealth", "2026-Q3");
        foreach (var section in package.Sections)
        {
            package.SubmitSectionForReview(section.Id);
            package.ApproveSection(section.Id);
        }

        package.StartCompilation();

        Assert.Equal(ReportPackageStatus.Compiling, package.Status);
    }

    [Fact]
    public void StartCompilation_ThrowsWhenPackageIsNotDraft()
    {
        var package = ReportPackage.Create("Acme Wealth", "2026-Q3");
        foreach (var section in package.Sections)
        {
            package.SubmitSectionForReview(section.Id);
            package.ApproveSection(section.Id);
        }
        package.StartCompilation();

        Assert.Throws<DomainInvariantViolationException>(package.StartCompilation);
    }

    [Fact]
    public void StartCompilation_ThrowsWhenPackageIsCompileFailed()
    {
        var package = ReportPackage.Create("Acme Wealth", "2026-Q3");
        foreach (var section in package.Sections)
        {
            package.SubmitSectionForReview(section.Id);
            package.ApproveSection(section.Id);
        }
        package.StartCompilation();   // Draft -> Compiling
        package.MarkCompileFailed();  // Compiling -> CompileFailed

        Assert.Throws<DomainInvariantViolationException>(package.StartCompilation);
    }


    [Fact]
    public void MarkCompiled_MovesToCompiled_WhenCompiling()
    {
        var package = ReportPackage.Create("Acme Wealth", "2026-Q3");
        foreach (var section in package.Sections)
        {
            package.SubmitSectionForReview(section.Id);
            package.ApproveSection(section.Id);
        }
        package.StartCompilation();

        package.MarkCompiled();

        Assert.Equal(ReportPackageStatus.Compiled, package.Status);
    }

    [Fact]
    public void MarkCompiled_ThrowsWhenNotCompiling()
    {
        var package = ReportPackage.Create("Acme Wealth", "2026-Q3");

        Assert.Throws<DomainInvariantViolationException>(package.MarkCompiled);
    }

    [Fact]
    public void MarkCompileFailed_MovesToCompileFailed_WhenCompiling()
    {
        var package = ReportPackage.Create("Acme Wealth", "2026-Q3");
        foreach (var section in package.Sections)
        {
            package.SubmitSectionForReview(section.Id);
            package.ApproveSection(section.Id);
        }
        package.StartCompilation();

        package.MarkCompileFailed();

        Assert.Equal(ReportPackageStatus.CompileFailed, package.Status);
    }

    [Fact]  
    public void MarkCompileFailed_ThrowsWhenNotCompiling()
    {
        var package = ReportPackage.Create("Acme Wealth", "2026-Q3");

        Assert.Throws<DomainInvariantViolationException>(package.MarkCompileFailed);
    }

    [Fact]
    public void RetryCompilation_MovesToDraftFromCompileFailed()
    {
        var package = ReportPackage.Create("Acme Wealth", "2026-Q3");
        foreach (var section in package.Sections)
        {
            package.SubmitSectionForReview(section.Id);
            package.ApproveSection(section.Id);
        }
        package.StartCompilation();
        package.MarkCompileFailed();

        package.RetryCompilation();

        Assert.Equal(ReportPackageStatus.Draft, package.Status);
    }

    [Fact]
    public void RetryCompilation_ThrowsWhenNotFailedToCompile()
    {
        var package = ReportPackage.Create("Acme Wealth", "2026-Q3");

        Assert.Throws<DomainInvariantViolationException>(package.RetryCompilation);
    }

    [Fact]
    public void Publish_MovesToPublishFromCompiled()
    {
        var package = ReportPackage.Create("Acme Wealth", "2026-Q3");
        foreach (var section in package.Sections)
        {
            package.SubmitSectionForReview(section.Id);
            package.ApproveSection(section.Id);
        }
        package.StartCompilation();
        package.MarkCompiled();

        package.Publish();
        Assert.Equal(ReportPackageStatus.Published, package.Status);
    }

    [Fact]
    public void Publish_ThrowsWhenNotCompiled()
    {
        var package = ReportPackage.Create("Acme Wealth", "2026-Q3");

        Assert.Throws<DomainInvariantViolationException>(package.Publish);
    }

    [Fact]
    public void SetSectionContent_UpdatesContent_WhenPending()
    {
        var package = ReportPackage.Create("Acme Wealth", "2026-Q3");
        var section = package.Sections.First(s => s.Type == SectionType.Commentary);

        package.SetSectionContent(section.Id, "Markets were resilient this quarter.");

        var updated = package.Sections.Single(s => s.Id == section.Id);
        Assert.Equal("Markets were resilient this quarter.", updated.Content);
    }

    [Fact]
    public void SetSectionContent_ThrowsWhenSectionIsNotPending()
    {
        var package = ReportPackage.Create("Acme Wealth", "2026-Q3");
        var section = package.Sections.First(s => s.Type == SectionType.Commentary);
        package.SubmitSectionForReview(section.Id); // now InReview

        Assert.Throws<DomainInvariantViolationException>(
            () => package.SetSectionContent(section.Id, "Edited while under review"));
    }
}