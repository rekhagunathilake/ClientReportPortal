using MediatR;

namespace ClientReportPortal.Application.ReportPackages;

public sealed record SetSectionContentCommand(Guid ReportPackageId, Guid SectionId, string Content) : IRequest;