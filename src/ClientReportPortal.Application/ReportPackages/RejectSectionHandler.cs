using MediatR;

namespace ClientReportPortal.Application.ReportPackages;

public sealed class RejectSectionHandler(IReportPackageRepository repository)
    : IRequestHandler<RejectSectionCommand>
{
    public async Task Handle(RejectSectionCommand request, CancellationToken cancellationToken)
    {
        var package = await repository.GetAsync(request.ReportPackageId, cancellationToken)
                      ?? throw new NotFoundException($"Report package {request.ReportPackageId} not found.");

        package.RejectSection(request.SectionId);
        await repository.SaveAsync(package, cancellationToken);
    }
}