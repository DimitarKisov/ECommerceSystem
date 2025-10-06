using Microsoft.EntityFrameworkCore;
using OrderManagement.Application.Common;
using OrderManagement.Application.DTOs;
using OrderManagement.Domain.Entities;
using OrderManagement.Infrastructure.Data;

namespace OrderManagement.Infrastructure.Services
{
    /// <summary>
    /// Service за работа с Orders
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
                .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);
        }

        /// <summary>
        /// ЗА COMMANDS: Зарежда Order entity с Items за domain операции
        /// Използва се когато трябва да извикаме domain методи: order.Confirm(), order.Cancel() и т.н.
        /// 
        /// ВАЖНО: Entity-то е tracked от EF Core, така че при SaveChanges()
        /// ще се update-нат САМО променените колони (напр. само Status)
        /// </summary>
        public async Task<Order?> GetByIdWithItemsAsync(Guid orderId, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);
        }

        public async Task<Order?> GetByNumberAsync(string orderNumber, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Orders
                .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber, cancellationToken);
        }

        public async Task<Order?> GetByNumberWithItemsAsync(string orderNumber, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber, cancellationToken);
        }

        public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task<OrderDto?> GetOrderDtoByIdAsync(Guid orderId, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Orders
                .Where(o => o.Id == orderId)
                .Select(o => new OrderDto
                {
                    Id = o.Id,
                    OrderNumber = o.OrderNumber,
                    CustomerId = o.CustomerId,
                    OrderDate = o.OrderDate,
                    Status = o.Status.ToString(),
                    ShippingAddress = new AddressDto
                    {
                        Street = o.ShippingAddress.Street,
                        City = o.ShippingAddress.City,
                        PostalCode = o.ShippingAddress.PostalCode,
                        Country = o.ShippingAddress.Country
                    },
                    TotalAmount = o.TotalAmount.Amount,
                    Currency = o.TotalAmount.Currency,
                    ShippedDate = o.ShippedDate,
                    DeliveredDate = o.DeliveredDate,
                    CancellationReason = o.CancellationReason,
                    Items = o.Items.Select(i => new OrderItemDto
                    {
                        Id = i.Id,
                        ProductId = i.ProductId,
                        ProductName = i.ProductName,
                        UnitPrice = i.UnitPrice.Amount,
                        Quantity = i.Quantity,
                        Subtotal = i.Subtotal.Amount
                    }).ToList()
                })
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<OrderDto?> GetOrderDtoByNumberAsync(string orderNumber, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Orders
                .Where(o => o.OrderNumber == orderNumber)
                .Select(o => new OrderDto
                {
                    Id = o.Id,
                    OrderNumber = o.OrderNumber,
                    CustomerId = o.CustomerId,
                    OrderDate = o.OrderDate,
                    Status = o.Status.ToString(),
                    ShippingAddress = new AddressDto
                    {
                        Street = o.ShippingAddress.Street,
                        City = o.ShippingAddress.City,
                        PostalCode = o.ShippingAddress.PostalCode,
                        Country = o.ShippingAddress.Country
                    },
                    TotalAmount = o.TotalAmount.Amount,
                    Currency = o.TotalAmount.Currency,
                    ShippedDate = o.ShippedDate,
                    DeliveredDate = o.DeliveredDate,
                    CancellationReason = o.CancellationReason,
                    Items = o.Items.Select(i => new OrderItemDto
                    {
                        Id = i.Id,
                        ProductId = i.ProductId,
                        ProductName = i.ProductName,
                        UnitPrice = i.UnitPrice.Amount,
                        Quantity = i.Quantity,
                        Subtotal = i.Subtotal.Amount
                    }).ToList()
                })
                .FirstOrDefaultAsync(cancellationToken);
        }

        /// <summary>
        /// Извлича САМО необходимата информация за summary view
        /// </summary>
        public async Task<List<OrderSummaryDto>> GetOrdersByCustomerIdAsync(
            Guid customerId,
            CancellationToken cancellationToken = default)
        {
            return await _dbContext.Orders
                .Where(o => o.CustomerId == customerId)
                .OrderByDescending(o => o.OrderDate)
                .Select(o => new OrderSummaryDto
                {
                    Id = o.Id,
                    OrderNumber = o.OrderNumber,
                    OrderDate = o.OrderDate,
                    Status = o.Status.ToString(),
                    TotalAmount = o.TotalAmount.Amount,
                    ItemsCount = o.Items.Count
                })
                .ToListAsync(cancellationToken);
        }
    }
}