using MediatR;

namespace ClientReportPortal.Application.ReportPackages;

public sealed class ReviseSectionHandler(IReportPackageRepository repository)
    : IRequestHandler<ReviseSectionCommand>
{
    public async Task Handle(ReviseSectionCommand request, CancellationToken cancellationToken)
    {
        var package = await repository.GetAsync(request.ReportPackageId, cancellationToken)
                      ?? throw new NotFoundException($"Report package {request.ReportPackageId} not found.");

        package.ReviseSection(request.SectionId);
        await repository.SaveAsync(package, cancellationToken);
    }
}