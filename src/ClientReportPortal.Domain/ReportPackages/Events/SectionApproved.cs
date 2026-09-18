using ClientReportPortal.Domain.Common;

namespace ClientReportPortal.Domain.ReportPackages.Events;

public sealed record SectionApproved(
    Guid ReportPackageId,
    Guid SectionId,
    SectionType SectionType,
    DateTime OccurredOnUtc) :
    IDomainEvent;
