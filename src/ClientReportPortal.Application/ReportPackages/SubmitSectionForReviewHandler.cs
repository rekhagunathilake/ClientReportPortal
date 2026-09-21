using MediatR;

namespace ClientReportPortal.Application.ReportPackages;

public sealed class SubmitSectionForReviewHandler(IReportPackageRepository repository)
    : IRequestHandler<SubmitSectionForReviewCommand>
{
    public async Task Handle(SubmitSectionForReviewCommand request, CancellationToken cancellationToken)
    {
        var package = await repository.GetAsync(request.ReportPackageId, cancellationToken)
                      ?? throw new NotFoundException($"Report package {request.ReportPackageId} not found.");

        package.SubmitSectionForReview(request.SectionId);
        await repository.SaveAsync(package, cancellationToken);
    }
}