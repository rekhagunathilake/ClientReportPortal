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
}