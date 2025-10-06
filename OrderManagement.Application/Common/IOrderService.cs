using OrderManagement.Application.DTOs;
using OrderManagement.Domain.Entities;

namespace OrderManagement.Application.Common
{
    /// <summary>
    /// Интерфейс за работа с Orders
    /// Разделение на Command (write) и Query (read) методи
    /// </summary>
    public interface IOrderService
    {
        // ========================================
        // Commands (Write) - Връщат entities за да можем да извикваме domain методи
        // ========================================

        void Add(Order order);

        /// <summary>
        /// За Commands - Зарежда Order entity с Items за да извикваме domain методи
        /// Използва се в: ConfirmOrderCommand, CancelOrderCommand, ShipOrderCommand, DeliverOrderCommand
        /// </summary>
        Task<Order> GetByIdWithItemsAsync(Guid orderId, CancellationToken cancellationToken = default);

        Task<Order> GetByIdAsync(Guid orderId, CancellationToken cancellationToken = default);
        Task<Order> GetByNumberAsync(string orderNumber, CancellationToken cancellationToken = default);
        Task<Order> GetByNumberWithItemsAsync(string orderNumber, CancellationToken cancellationToken = default);
        Task SaveChangesAsync(CancellationToken cancellationToken = default);

        // ========================================
        // Queries (Read) - Оптимизирани с projection към DTOs
        // ========================================

        /// <summary>
        /// За Queries - Оптимизиран метод с direct projection към DTO
        /// Използва се в: GetOrderByIdQuery, GetOrderByNumberQuery
        /// Извлича САМО необходимите колони без да материализира Order entity
        /// </summary>
        Task<OrderDto?> GetOrderDtoByIdAsync(Guid orderId, CancellationToken cancellationToken = default);

        /// <summary>
        /// За Queries - Projection към DTO по Order Number
        /// </summary>
        Task<OrderDto?> GetOrderDtoByNumberAsync(string orderNumber, CancellationToken cancellationToken = default);

        /// <summary>
        /// За Queries - Кратка информация за поръчките на клиент (summaries)
        /// </summary>
        Task<List<OrderSummaryDto>> GetOrdersByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default);
    }
}