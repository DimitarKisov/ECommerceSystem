using Microsoft.EntityFrameworkCore;
using OrderManagement.Application.Common;
using OrderManagement.Domain.Entities;
using OrderManagement.Infrastructure.Data;

namespace OrderManagement.Infrastructure.Repositories
{
    /// <summary>
    /// Repository за работа с Orders
    /// Съдържа цялата логика за манипулиране на данни
    /// </summary>
    public class OrderService : IOrderService
    {
        private readonly OrderManagementDbContext _dbContext;

        public OrderService(OrderManagementDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public void Add(Order order)
        {
            _dbContext.Orders.Add(order);
        }

        public async Task<Order?> GetByIdAsync(Guid orderId, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Orders
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);
        }

        public async Task<Order?> GetByIdWithItemsAsync(Guid orderId, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);
        }

        public async Task<Order?> GetByNumberAsync(string orderNumber, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Orders
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber, cancellationToken);
        }

        public async Task<Order?> GetByNumberWithItemsAsync(string orderNumber, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Orders
                .Include(o => o.Items)
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber, cancellationToken);
        }

        public async Task<List<Order>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Orders
                .Include(o => o.Items)
                .AsNoTracking()
                .Where(o => o.CustomerId == customerId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync(cancellationToken);
        }

        public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}