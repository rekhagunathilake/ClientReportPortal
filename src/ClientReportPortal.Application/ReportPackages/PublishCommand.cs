using MediatR;

namespace ClientReportPortal.Application.ReportPackages;

public sealed record PublishCommand(Guid ReportPackageId) : IRequest;