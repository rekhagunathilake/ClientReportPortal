using MediatR;

namespace ClientReportPortal.Application.ReportPackages;

public sealed record SubmitSectionForReviewCommand(Guid ReportPackageId, Guid SectionId) : IRequest;
