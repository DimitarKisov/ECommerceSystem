using Microsoft.EntityFrameworkCore;
using OrderManagement.Domain.Entities;
using OrderManagement.Domain.Common;
using MediatR;
using System.Reflection;

namespace OrderManagement.Infrastructure.Data
{
    /// <summary>
    /// Database context за Order Management
    /// </summary>
    public class OrderManagementDbContext : DbContext
    {
        private readonly IMediator _mediator;

        public OrderManagementDbContext(DbContextOptions<OrderManagementDbContext> options, IMediator mediator)
            : base(options)
        {
            _mediator = mediator;
        }

        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Прилагаме всички entity configurations от текущия assembly
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }

        /// <summary>
        /// Override на SaveChangesAsync за dispatch на domain events
        /// </summary>
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // Dispatch domain events преди да запишем промените
            await DispatchDomainEventsAsync();

            return await base.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// Изпраща всички domain events чрез MediatR
        /// </summary>
        private async Task DispatchDomainEventsAsync()
        {
            var domainEntities = ChangeTracker
                .Entries<BaseEntity>()
                .Where(x => x.Entity.DomainEvents.Any())
                .Select(x => x.Entity)
                .ToList();

            var domainEvents = domainEntities
                .SelectMany(x => x.DomainEvents)
                .ToList();

            // Изчистваме events от entities
            domainEntities.ForEach(entity => entity.ClearDomainEvents());

            // Публикуваме events чрез MediatR
            foreach (var domainEvent in domainEvents)
            {
                await _mediator.Publish(domainEvent);
            }
        }
    }
}