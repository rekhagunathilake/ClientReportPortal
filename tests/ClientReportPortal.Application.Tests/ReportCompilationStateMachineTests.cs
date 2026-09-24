using ClientReportPortal.Application.Compilation;
using ClientReportPortal.Application.Compilation.Events;
using ClientReportPortal.Application.ReportPackages;
using ClientReportPortal.Domain.ReportPackages;
using FluentAssertions;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace ClientReportPortal.Application.Tests;

public class ReportCompilationStateMachineTests
{
    [Fact]
    public async Task CompilationRequested_PublishesPerformanceDataFetched()
    {
        var repository = new FakeReportPackageRepository();
        await using var provider = new ServiceCollection()
            .AddMassTransitTestHarness(cfg =>
            {
                cfg.AddSagaStateMachine<ReportCompilationStateMachine, ReportCompilationState>()
                    .InMemoryRepository();
            })
            .AddSingleton<IPerformanceDataProvider, FakePerformanceDataProvider>()
            .AddSingleton<ISectionRenderer, FakeSectionRenderer>()
            .AddSingleton<IReportPdfAssembler, FakeReportPdfAssembler>()
            .AddSingleton<IReportPackageStorage, FakeReportPackageStorage>()
            .AddSingleton<IReportPackageRepository>(repository)
            .BuildServiceProvider(true);

        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        try
        {
            var reportPackageId = Guid.NewGuid();
            await harness.Bus.Publish(new CompilationRequested(reportPackageId));

            (await harness.Consumed.Any<CompilationRequested>()).Should().BeTrue();
            (await harness.Published.Any<PerformanceDataFetched>()).Should().BeTrue();
        }
        finally
        {
            await harness.Stop();
        }
    }

    [Fact]
    public async Task PerformanceDataFetched_TransitionsSagaToRenderingSections()
    {
        var repository = new FakeReportPackageRepository();
        await using var provider = new ServiceCollection()
            .AddMassTransitTestHarness(cfg =>
            {
                cfg.AddSagaStateMachine<ReportCompilationStateMachine, ReportCompilationState>()
                    .InMemoryRepository();
            })
            .AddSingleton<IPerformanceDataProvider, FakePerformanceDataProvider>()
            .AddSingleton<ISectionRenderer, FakeSectionRenderer>()
            .AddSingleton<IReportPdfAssembler, FakeReportPdfAssembler>()
            .AddSingleton<IReportPackageStorage, FakeReportPackageStorage>()
            .AddSingleton<IReportPackageRepository>(repository)
            .BuildServiceProvider(true);

        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        try
        {
            var reportPackageId = Guid.NewGuid();
            await harness.Bus.Publish(new CompilationRequested(reportPackageId));

            (await harness.Published.Any<SectionsRendered>()).Should().BeTrue();
        }
        finally
        {
            await harness.Stop();
        }
    }

    [Fact]
    public async Task SectionsRendered_TransitionsSagaToAssemblingPdf()
    {
        var repository = new FakeReportPackageRepository();
        await using var provider = new ServiceCollection()
            .AddMassTransitTestHarness(cfg =>
            {
                cfg.AddSagaStateMachine<ReportCompilationStateMachine, ReportCompilationState>()
                    .InMemoryRepository();
            })
            .AddSingleton<IPerformanceDataProvider, FakePerformanceDataProvider>()
            .AddSingleton<ISectionRenderer, FakeSectionRenderer>()
            .AddSingleton<IReportPdfAssembler, FakeReportPdfAssembler>()
            .AddSingleton<IReportPackageStorage, FakeReportPackageStorage>()
            .AddSingleton<IReportPackageRepository>(repository)
            .BuildServiceProvider(true);

        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        try
        {
            var reportPackageId = Guid.NewGuid();
            await harness.Bus.Publish(new CompilationRequested(reportPackageId));

            (await harness.Published.Any<PdfAssembled>()).Should().BeTrue();
        }
        finally
        {
            await harness.Stop();
        }
    }

    [Fact]
    public async Task PdfAssembled_TransitionsSagaToStoringPdf()
    {
        var repository = new FakeReportPackageRepository();
        await using var provider = new ServiceCollection()
            .AddMassTransitTestHarness(cfg =>
            {
                cfg.AddSagaStateMachine<ReportCompilationStateMachine, ReportCompilationState>()
                    .InMemoryRepository();
            })
            .AddSingleton<IPerformanceDataProvider, FakePerformanceDataProvider>()
            .AddSingleton<ISectionRenderer, FakeSectionRenderer>()
            .AddSingleton<IReportPdfAssembler, FakeReportPdfAssembler>()
            .AddSingleton<IReportPackageStorage, FakeReportPackageStorage>()
            .AddSingleton<IReportPackageRepository>(repository)
            .BuildServiceProvider(true);

        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        try
        {
            var reportPackageId = Guid.NewGuid();
            await harness.Bus.Publish(new CompilationRequested(reportPackageId));

            (await harness.Published.Any<PdfStored>()).Should().BeTrue();
        }
        finally
        {
            await harness.Stop();
        }
    }

    [Fact]
    public async Task PdfStored_MarksReportPackageCompiled()
    {
        var repository = new FakeReportPackageRepository();
        var package = ReportPackage.Create("Acme Wealth", "2026-Q3");
        foreach (var section in package.Sections)
        {
            package.SubmitSectionForReview(section.Id);
            package.ApproveSection(section.Id);
        }
        package.StartCompilation();
        repository.Seed(package);

        await using var provider = new ServiceCollection()
            .AddMassTransitTestHarness(cfg =>
            {
                cfg.AddSagaStateMachine<ReportCompilationStateMachine, ReportCompilationState>()
                    .InMemoryRepository();
            })
            .AddSingleton<IPerformanceDataProvider, FakePerformanceDataProvider>()
            .AddSingleton<ISectionRenderer, FakeSectionRenderer>()
            .AddSingleton<IReportPdfAssembler, FakeReportPdfAssembler>()
            .AddSingleton<IReportPackageStorage, FakeReportPackageStorage>()
            .AddSingleton<IReportPackageRepository>(repository)
            .BuildServiceProvider(true);

        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        try
        {
            await harness.Bus.Publish(new CompilationRequested(package.Id));

            (await harness.Consumed.Any<PdfStored>()).Should().BeTrue();

            repository.Saved!.Status.Should().Be(ReportPackageStatus.Compiled);
        }
        finally
        {
            await harness.Stop();
        }
    }

    [Fact]
    public async Task CompilationRequested_WhenFetchingPerformanceDataFails_MarksReportPackageCompileFailed()
    {
        var repository = new FakeReportPackageRepository();
        var package = ReportPackage.Create("Acme Wealth", "2026-Q3");
        foreach (var section in package.Sections)
        {
            package.SubmitSectionForReview(section.Id);
            package.ApproveSection(section.Id);
        }
        package.StartCompilation();
        repository.Seed(package);

        await using var provider = new ServiceCollection()
            .AddMassTransitTestHarness(cfg =>
            {
                cfg.AddSagaStateMachine<ReportCompilationStateMachine, ReportCompilationState>()
                    .InMemoryRepository();
            })
            .AddSingleton<IPerformanceDataProvider, ThrowingPerformanceDataProvider>()
            .AddSingleton<ISectionRenderer, FakeSectionRenderer>()
            .AddSingleton<IReportPdfAssembler, FakeReportPdfAssembler>()
            .AddSingleton<IReportPackageStorage, FakeReportPackageStorage>()
            .AddSingleton<IReportPackageRepository>(repository)
            .BuildServiceProvider(true);

        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        try
        {
            await harness.Bus.Publish(new CompilationRequested(package.Id));

            (await harness.Published.Any<CompilationFailed>()).Should().BeTrue();
            repository.Saved!.Status.Should().Be(ReportPackageStatus.CompileFailed);
        }
        finally
        {
            await harness.Stop();
        }
    }
}