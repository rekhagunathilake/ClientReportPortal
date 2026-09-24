using ClientReportPortal.Application.Compilation.Events;
using ClientReportPortal.Application.ReportPackages;
using MassTransit;

namespace ClientReportPortal.Application.Compilation;

public sealed class ReportCompilationStateMachine : MassTransitStateMachine<ReportCompilationState>
{
    public State FetchingPerformanceData { get; private set; } = default!;

    public Event<CompilationRequested> CompilationRequested { get; private set; } = default!;
    public Event<PerformanceDataFetched> PerformanceDataFetched { get; private set; } = default!;
    public State RenderingSections { get; private set; } = default!;
    public Event<SectionsRendered> SectionsRendered { get; private set; } = default!;
    public State AssemblingPdf { get; private set; } = default!;
    public Event<PdfAssembled> PdfAssembled { get; private set; } = default!;
    public State StoringPdf { get; private set; } = default!;
    public Event<PdfStored> PdfStored { get; private set; } = default!;
    public Event<CompilationFailed> CompilationFailed { get; private set; } = default!;

    public ReportCompilationStateMachine(
        IPerformanceDataProvider performanceDataProvider, 
        ISectionRenderer sectionRenderer, 
        IReportPdfAssembler reportPdfAssembler,
        IReportPackageStorage reportPackageStorage,
        IReportPackageRepository reportPackageRepository)
    {
        InstanceState(x => x.CurrentState);

        Event(() => CompilationRequested, x => x.CorrelateById(m => m.Message.ReportPackageId));
        Event(() => PerformanceDataFetched, x => x.CorrelateById(m => m.Message.ReportPackageId));
        Event(() => SectionsRendered, x => x.CorrelateById(m => m.Message.ReportPackageId));
        Event(() => PdfAssembled, x => x.CorrelateById(m => m.Message.ReportPackageId));
        Event(() => PdfStored, x => x.CorrelateById(m => m.Message.ReportPackageId));
        Event(() => CompilationFailed, x => x.CorrelateById(m => m.Message.ReportPackageId));

        Initially(
            When(CompilationRequested)
                .Then(context => context.Saga.ReportPackageId = context.Message.ReportPackageId)
                .ThenAsync(async context =>
                {
                    try
                    {
                        await performanceDataProvider.GetAsync(context.Saga.ReportPackageId, context.CancellationToken);
                        await context.Publish(new PerformanceDataFetched(context.Saga.ReportPackageId));
                    }
                    catch (Exception)
                    {
                        await MarkPackageCompileFailedAsync(reportPackageRepository, context.Saga.ReportPackageId, context.CancellationToken);
                        await context.Publish(new CompilationFailed(context.Saga.ReportPackageId));
                    }
                })
                .TransitionTo(FetchingPerformanceData)
        );

        During(FetchingPerformanceData,
            When(PerformanceDataFetched)
                .ThenAsync(async context =>
                {
                    try
                    {
                        await sectionRenderer.RenderAsync(context.Saga.ReportPackageId, context.CancellationToken);
                        await context.Publish(new SectionsRendered(context.Saga.ReportPackageId));
                    }
                    catch (Exception)
                    {
                        await MarkPackageCompileFailedAsync(reportPackageRepository, context.Saga.ReportPackageId, context.CancellationToken);
                        await context.Publish(new CompilationFailed(context.Saga.ReportPackageId));
                    }
                })
                .TransitionTo(RenderingSections)
        );

        During(RenderingSections,
            When(SectionsRendered)
                .ThenAsync(async context =>
                {
                    try
                    {
                        await reportPdfAssembler.AssembleAsync(context.Saga.ReportPackageId, context.CancellationToken);
                        await context.Publish(new PdfAssembled(context.Saga.ReportPackageId));
                    }
                    catch (Exception)
                    {
                        await MarkPackageCompileFailedAsync(reportPackageRepository, context.Saga.ReportPackageId, context.CancellationToken);
                        await context.Publish(new CompilationFailed(context.Saga.ReportPackageId));
                    }
                })
                .TransitionTo(AssemblingPdf)
        );

        During(AssemblingPdf,
            When(PdfAssembled)
                .ThenAsync(async context =>
                {
                    try
                    {
                        await reportPackageStorage.StoreAsync(context.Saga.ReportPackageId, context.CancellationToken);
                        await context.Publish(new PdfStored(context.Saga.ReportPackageId));
                    }
                    catch (Exception)
                    {
                        await MarkPackageCompileFailedAsync(reportPackageRepository, context.Saga.ReportPackageId, context.CancellationToken);
                        await context.Publish(new CompilationFailed(context.Saga.ReportPackageId));
                    }
                })
                .TransitionTo(StoringPdf)
        );

        During(StoringPdf,
            When(PdfStored)
                .ThenAsync(async context =>
                {
                    try
                    {
                        var package = await reportPackageRepository.GetAsync(context.Saga.ReportPackageId, context.CancellationToken)
                                                  ?? throw new NotFoundException($"Report package {context.Saga.ReportPackageId} not found.");

                        package.MarkCompiled();
                        await reportPackageRepository.SaveAsync(package, context.CancellationToken);
                    }
                    catch (Exception)
                    {
                        // NOTE: if SaveAsync above throws *after* MarkCompiled() already mutated the
                        // in-memory package, this catch will try MarkCompileFailed() on an already-Compiled 
                        // package and throw. Not reachable with the current fakes (nothing
                        // fails SaveAsync independently of the work itself), but a real gap once
                        // Infrastructure has a database that could fail the save step alone.
                        await MarkPackageCompileFailedAsync(reportPackageRepository, context.Saga.ReportPackageId, context.CancellationToken);
                        await context.Publish(new CompilationFailed(context.Saga.ReportPackageId));
                    }
                })
                .TransitionTo(Final)
        );

        DuringAny(
            When(CompilationFailed)
                .TransitionTo(Final)
        );
    }

    private static async Task MarkPackageCompileFailedAsync(
        IReportPackageRepository repository, Guid reportPackageId, CancellationToken ct)
    {
        var package = await repository.GetAsync(reportPackageId, ct);
        if (package is not null)
        {
            package.MarkCompileFailed();
            await repository.SaveAsync(package, ct);
        }
    }
}
