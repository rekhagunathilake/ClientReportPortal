namespace ClientReportPortal.Domain.ReportPackages;

public sealed class Section
{
    public Guid Id { get; private init; }
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

    internal void SubmitForReview()
    {
        if (Status != SectionStatus.Pending)
            throw new DomainInvariantViolationException(
                $"Section {Id} must be Pending to be submitted for review (was {Status}).");

        Status = SectionStatus.InReview;
    }

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

    internal void SetContent(string content)
    {
        if (Status != SectionStatus.Pending)
            throw new DomainInvariantViolationException($"Cannot set content for section {Id} because it is not in the Pending state.");

        Content = content;
    }
}