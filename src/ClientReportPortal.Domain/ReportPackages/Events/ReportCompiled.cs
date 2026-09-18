using ClientReportPortal.Domain.Common;

namespace ClientReportPortal.Domain.ReportPackages.Events;

public sealed record ReportCompiled(Guid ReportPackageId, DateTime OccurredOnUtc) :
    IDomainEvent;
