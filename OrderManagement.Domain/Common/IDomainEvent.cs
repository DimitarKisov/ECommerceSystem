using MediatR;

namespace OrderManagement.Domain.Common
{
    /// <summary>
    /// Marker interface за domain events
    /// Наследява INotification за интеграция с MediatR
    /// </summary>
    public interface IDomainEvent : INotification
    {
        Guid EventId { get; }
        DateTime OccurredOn { get; }
    }
}