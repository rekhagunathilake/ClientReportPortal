namespace ClientReportPortal.Domain.Common;

public interface IDomainEvent
{
    DateTime OccurredOnUtc { get; }
}
