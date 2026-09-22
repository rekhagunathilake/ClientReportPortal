using MediatR;

namespace ClientReportPortal.Application.ReportPackages;

public sealed class PublishHandler(IReportPackageRepository repository)
    : IRequestHandler<PublishCommand>
{
    public async Task Handle(PublishCommand request, CancellationToken cancellationToken)
    {
        var package = await repository.GetAsync(request.ReportPackageId, cancellationToken)
                      ?? throw new NotFoundException($"Report package {request.ReportPackageId} not found.");

        package.Publish();
        await repository.SaveAsync(package, cancellationToken);
    }
}