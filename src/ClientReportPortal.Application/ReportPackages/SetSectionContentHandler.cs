using MediatR;

namespace ClientReportPortal.Application.ReportPackages;

public sealed class SetSectionContentHandler(IReportPackageRepository repository)
    : IRequestHandler<SetSectionContentCommand>
{
    public async Task Handle(SetSectionContentCommand request, CancellationToken cancellationToken)
    {
        var package = await repository.GetAsync(request.ReportPackageId, cancellationToken)
                      ?? throw new NotFoundException($"Report package {request.ReportPackageId} not found.");

        package.SetSectionContent(request.SectionId, request.Content);
        await repository.SaveAsync(package, cancellationToken);
    }
}