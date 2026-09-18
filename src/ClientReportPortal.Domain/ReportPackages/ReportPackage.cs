using ClientReportPortal.Domain.Common;

namespace ClientReportPortal.Domain.ReportPackages;

public sealed class ReportPackage : AggregateRoot
{
    private readonly List<Section> _sections = new();

    public Guid Id { get; private set; }
    public string ClientName { get; private set; } = default!;
    public string Quarter { get; private set; } = default!;
    public ReportPackageStatus Status { get; private set; }
    public IReadOnlyCollection<Section> Sections => _sections.AsReadOnly();

    private ReportPackage() { } // EF Core later

    public static ReportPackage Create(string clientName, string quarter)
    {
        var package = new ReportPackage
        {
            Id = Guid.NewGuid(),
            ClientName = clientName,
            Quarter = quarter,
            Status = ReportPackageStatus.Draft
        };

        foreach (var type in Enum.GetValues<SectionType>())
            package._sections.Add(Section.CreatePending(type));

        return package;
    }

    public void SubmitSectionForReview(Guid sectionId)
    {
        var section = _sections.SingleOrDefault(s => s.Id == sectionId)
                      ?? throw new DomainInvariantViolationException($"Section {sectionId} not found in package {Id}.");

        // Update the section status to InReview
        section.SubmitForReview();
    }

    public void ApproveSection(Guid sectionId)
    {
        var section = _sections.SingleOrDefault(s => s.Id == sectionId)
                      ?? throw new DomainInvariantViolationException($"Section {sectionId} not found in package {Id}.");

        // Update the section status to Approved
        section.Approve();
    }

    public void RejectSection(Guid sectionId)
    {
        var section = _sections.SingleOrDefault(s => s.Id == sectionId)
                      ?? throw new DomainInvariantViolationException($"Section {sectionId} not found in package {Id}.");
        
        // Update the section status to Rejected
        section.Reject();
    }

    public void ReviseSection(Guid sectionId)
    {
        var section = _sections.SingleOrDefault(s => s.Id == sectionId)
                      ?? throw new DomainInvariantViolationException($"Section {sectionId} not found in package {Id}.");

        // Update the section status to Pending
        section.Revise();
    }

    public void StartCompilation()
    {
        if (Status != ReportPackageStatus.Draft)
            throw new DomainInvariantViolationException(
                $"Cannot start compilation for package {Id} because it is not in the Draft state (was {Status}).");

        if (_sections.Any(s => s.Status != SectionStatus.Approved))
            throw new DomainInvariantViolationException($"Cannot start compilation for package {Id} because not all sections are approved.");

        Status = ReportPackageStatus.Compiling;
    }

    public void MarkCompiled()
    {
        if (Status != ReportPackageStatus.Compiling)
            throw new DomainInvariantViolationException($"Cannot mark package {Id} as compiled because it is not in the Compiling state.");
        Status = ReportPackageStatus.Compiled;
    }

    public void MarkCompileFailed()
    {
        if (Status != ReportPackageStatus.Compiling)
            throw new DomainInvariantViolationException($"Cannot mark package {Id} as compile-failed because it is not in the Compiling state.");
        Status = ReportPackageStatus.CompileFailed;
    }

    public void RetryCompilation()
    {
        if (Status != ReportPackageStatus.CompileFailed)
            throw new DomainInvariantViolationException($"Cannot retry compilation for package {Id} because it is not in the CompileFailed state.");
        Status = ReportPackageStatus.Draft;
    }

    public void Publish()
    {
        if (Status != ReportPackageStatus.Compiled)
            throw new DomainInvariantViolationException($"Cannot publish package {Id} because it is not in the Compiled state.");
        Status = ReportPackageStatus.Published;
    }

    public void SetSectionContent(Guid sectionId, string content)
    {
        var section = _sections.SingleOrDefault(s => s.Id == sectionId)
                      ?? throw new DomainInvariantViolationException($"Section {sectionId} not found in package {Id}.");

        section.SetContent(content);
    }
}
