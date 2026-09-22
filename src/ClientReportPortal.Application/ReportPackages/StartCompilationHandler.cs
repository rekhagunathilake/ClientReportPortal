using MediatR;

namespace ClientReportPortal.Application.ReportPackages;

public sealed class StartCompilationHandler(IReportPackageRepository repository)
    : IRequestHandler<StartCompilationCommand>
{
    public async Task Handle(StartCompilationCommand request, CancellationToken cancellationToken)
    {
        var package = await repository.GetAsync(request.ReportPackageId, cancellationToken)
                      ?? throw new NotFoundException($"Report package {request.ReportPackageId} not found.");

        package.StartCompilation();
        await repository.SaveAsync(package, cancellationToken);
    }
}