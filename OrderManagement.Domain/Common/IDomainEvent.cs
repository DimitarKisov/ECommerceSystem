namespace OrderManagement.Domain.Common
{
    /// <summary>
    /// Marker interface за domain events
    /// </summary>
    public interface IDomainEvent
    {
        Guid EventId { get; }
        DateTime OccurredOn { get; }
    }
}
