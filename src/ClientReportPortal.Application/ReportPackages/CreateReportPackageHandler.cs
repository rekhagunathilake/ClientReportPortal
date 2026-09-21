using ClientReportPortal.Domain.ReportPackages;
using MediatR;

namespace ClientReportPortal.Application.ReportPackages;

public sealed class CreateReportPackageHandler(IReportPackageRepository repository)
    : IRequestHandler<CreateReportPackageCommand, Guid>
{
    public async Task<Guid> Handle(CreateReportPackageCommand request, CancellationToken cancellationToken)
    {
        var package = ReportPackage.Create(request.ClientName, request.Quarter);
        await repository.AddAsync(package, cancellationToken);
        return package.Id;
    }
}