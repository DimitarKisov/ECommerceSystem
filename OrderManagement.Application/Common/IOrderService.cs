using OrderManagement.Domain.Entities;

namespace OrderManagement.Application.Common
{
    /// <summary>
    /// Интерфейс за работа с Orders
    /// </summary>
    public interface IOrderService
    {
        // Commands (Write)
        void Add(Order order);
        Task<Order> GetByIdAsync(Guid orderId, CancellationToken cancellationToken = default);
        Task<Order> GetByIdWithItemsAsync(Guid orderId, CancellationToken cancellationToken = default);
        Task<Order> GetByNumberAsync(string orderNumber, CancellationToken cancellationToken = default);
        Task<Order> GetByNumberWithItemsAsync(string orderNumber, CancellationToken cancellationToken = default);

        // Queries (Read)
        Task<List<Order>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default);
        Task SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}