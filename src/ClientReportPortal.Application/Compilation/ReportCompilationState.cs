using MassTransit;

namespace ClientReportPortal.Application.Compilation;

public sealed class ReportCompilationState : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; } = default!;
    public Guid ReportPackageId { get; set; }
}