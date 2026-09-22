using MediatR;

namespace ClientReportPortal.Application.ReportPackages;

public sealed record RetryCompilationCommand(Guid ReportPackageId) : IRequest;