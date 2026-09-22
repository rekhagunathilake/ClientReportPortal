using MediatR;

namespace ClientReportPortal.Application.ReportPackages;

public sealed record StartCompilationCommand(Guid ReportPackageId) : IRequest;