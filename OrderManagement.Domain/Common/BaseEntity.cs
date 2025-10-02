namespace OrderManagement.Domain.Common
{
    /// <summary>
    /// Базов клас за всички entity обекти в домейна
    /// </summary>
    public abstract class BaseEntity
    {
        public Guid Id { get; protected set; }

        /// <summary>
        /// Колекция от domain events, които са възникнали за това entity
        /// </summary>
        private readonly List<IDomainEvent> _domainEvents = new();

        public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

        protected void AddDomainEvent(IDomainEvent domainEvent)
        {
            _domainEvents.Add(domainEvent);
        }

        public void ClearDomainEvents()
        {
            _domainEvents.Clear();
        }
    }
}
