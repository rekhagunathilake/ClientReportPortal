namespace ClientReportPortal.Domain.ReportPackages;
public sealed class Section
{
    public Guid Id { get; private set; }
    public SectionType Type { get; private set; }
    public SectionStatus Status { get; private set; }
    public string? Content { get; private set; }

    private Section() { } // EF Core later

    internal static Section CreatePending(SectionType type)
    {
        return new Section
        {
            Id = Guid.NewGuid(),
            Type = type,
            Status = SectionStatus.Pending,
            Content = null
        };
    }

    internal void SubmitForReview() => Status = SectionStatus.InReview;

    internal void Approve()
    {
        if (Status != SectionStatus.InReview)
            throw new DomainInvariantViolationException($"Section {Id} must be InReview to be approved (was {Status}).");
        Status = SectionStatus.Approved;
    }

    internal void Reject()
    {
        if (Status != SectionStatus.InReview)
            throw new DomainInvariantViolationException($"Section {Id} must be InReview to be rejected (was {Status}).");
        Status = SectionStatus.Rejected;
    }

    internal void Revise()
    {
        if (Status != SectionStatus.Rejected)
            throw new DomainInvariantViolationException($"Section {Id} must be Rejected to be revised (was {Status}).");
        Status = SectionStatus.Pending;
    }
}