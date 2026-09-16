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
}