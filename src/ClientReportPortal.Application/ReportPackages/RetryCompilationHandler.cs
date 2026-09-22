using MediatR;

namespace ClientReportPortal.Application.ReportPackages;

public sealed class RetryCompilationHandler(IReportPackageRepository repository)
    : IRequestHandler<RetryCompilationCommand>
{
    public async Task Handle(RetryCompilationCommand request, CancellationToken cancellationToken)
    {
        var package = await repository.GetAsync(request.ReportPackageId, cancellationToken)
                      ?? throw new NotFoundException($"Report package {request.ReportPackageId} not found.");

        package.RetryCompilation();
        await repository.SaveAsync(package, cancellationToken);
    }
}