using MediatR;

namespace ClientReportPortal.Application.ReportPackages;

public sealed class ApproveSectionHandler(IReportPackageRepository repository)
    : IRequestHandler<ApproveSectionCommand>
{
    public async Task Handle(ApproveSectionCommand request, CancellationToken cancellationToken)
    {
        var package = await repository.GetAsync(request.ReportPackageId, cancellationToken)
                      ?? throw new NotFoundException($"Report package {request.ReportPackageId} not found.");

        package.ApproveSection(request.SectionId);
        await repository.SaveAsync(package, cancellationToken);
    }
}