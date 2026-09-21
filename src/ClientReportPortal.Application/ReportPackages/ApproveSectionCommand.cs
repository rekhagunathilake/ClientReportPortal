using MediatR;

namespace ClientReportPortal.Application.ReportPackages;
public sealed record ApproveSectionCommand(Guid ReportPackageId, Guid SectionId) : IRequest;