using Dbets.Domain.Mediator;

namespace Dbets.Domain.Common;

public abstract record DomainEvent : INotification
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}