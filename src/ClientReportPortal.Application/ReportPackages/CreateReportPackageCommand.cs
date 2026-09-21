using MediatR;

namespace ClientReportPortal.Application.ReportPackages;

public sealed record CreateReportPackageCommand(string ClientName, string Quarter) : IRequest<Guid>;