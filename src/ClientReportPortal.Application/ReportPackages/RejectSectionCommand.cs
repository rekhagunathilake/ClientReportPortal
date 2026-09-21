using MediatR;

namespace ClientReportPortal.Application.ReportPackages;

public sealed record RejectSectionCommand(Guid ReportPackageId, Guid SectionId) : IRequest;