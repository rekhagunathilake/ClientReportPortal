using ClientReportPortal.Application.Compilation;
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
        await using var provider = new ServiceCollection()
            .AddMassTransitTestHarness(cfg =>
            {
                cfg.AddSagaStateMachine<ReportCompilationStateMachine, ReportCompilationState>()
                    .InMemoryRepository();
            })
            .AddSingleton<IPerformanceDataProvider, FakePerformanceDataProvider>()
            .AddSingleton<ISectionRenderer, FakeSectionRenderer>()
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
        await using var provider = new ServiceCollection()
            .AddMassTransitTestHarness(cfg =>
            {
                cfg.AddSagaStateMachine<ReportCompilationStateMachine, ReportCompilationState>()
                    .InMemoryRepository();
            })
            .AddSingleton<IPerformanceDataProvider, FakePerformanceDataProvider>()
            .AddSingleton<ISectionRenderer, FakeSectionRenderer>()
            .BuildServiceProvider(true);

        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        try
        {
            var reportPackageId = Guid.NewGuid();
            await harness.Bus.Publish(new CompilationRequested(reportPackageId));

            (await harness.Published.Any<SectionsRendered>()).Should().BeTrue();

            var sagaHarness = harness.GetSagaStateMachineHarness<ReportCompilationStateMachine, ReportCompilationState>();
            var instanceId = sagaHarness.Created.ContainsInState(
                reportPackageId, sagaHarness.StateMachine, sagaHarness.StateMachine.RenderingSections);

            instanceId.Should().NotBeNull();
        }
        finally
        {
            await harness.Stop();
        }
    }
}