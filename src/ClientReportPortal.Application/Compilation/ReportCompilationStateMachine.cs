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

        Initially(
            When(CompilationRequested)
                .Then(context => context.Saga.ReportPackageId = context.Message.ReportPackageId)
                .ThenAsync(async context =>
                {
                    await performanceDataProvider.GetAsync(context.Saga.ReportPackageId, context.CancellationToken);
                    await context.Publish(new PerformanceDataFetched(context.Saga.ReportPackageId));
                })
                .TransitionTo(FetchingPerformanceData)
        );

        During(FetchingPerformanceData,
            When(PerformanceDataFetched)
                .ThenAsync(async context =>
                {
                    await sectionRenderer.RenderAsync(context.Saga.ReportPackageId, context.CancellationToken);
                    await context.Publish(new SectionsRendered(context.Saga.ReportPackageId));
                })
                .TransitionTo(RenderingSections)
        );

        During(RenderingSections,
            When(SectionsRendered)
                .ThenAsync(async context =>
                {
                    await reportPdfAssembler.AssembleAsync(context.Saga.ReportPackageId, context.CancellationToken);
                    await context.Publish(new PdfAssembled(context.Saga.ReportPackageId));
                })
                .TransitionTo(AssemblingPdf)
        );

        During(AssemblingPdf,
            When(PdfAssembled)
                .ThenAsync(async context =>
                {
                    await reportPackageStorage.StoreAsync(context.Saga.ReportPackageId, context.CancellationToken);
                    await context.Publish(new PdfStored(context.Saga.ReportPackageId));
                })
                .TransitionTo(StoringPdf)
        );

        During(StoringPdf,
            When(PdfStored)
                .ThenAsync(async context =>
                {
                    var package = await reportPackageRepository.GetAsync(context.Saga.ReportPackageId, context.CancellationToken)
                                  ?? throw new NotFoundException($"Report package {context.Saga.ReportPackageId} not found.");

                    package.MarkCompiled();
                    await reportPackageRepository.SaveAsync(package, context.CancellationToken);
                })
                .TransitionTo(Final)
        );
    }
}
