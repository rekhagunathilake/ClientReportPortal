using ClientReportPortal.Domain.Common;

namespace ClientReportPortal.Domain.ReportPackages.Events;
public sealed record ReportPublished(
    Guid ReportPackageId,
    DateTime OccurredOnUtc) :
    IDomainEvent;
