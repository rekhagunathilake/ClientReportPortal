using MediatR;

namespace ClientReportPortal.Application.ReportPackages;

public sealed record ReviseSectionCommand(Guid ReportPackageId, Guid SectionId) : IRequest;