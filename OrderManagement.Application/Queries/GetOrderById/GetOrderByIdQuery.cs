using MediatR;
using OrderManagement.Application.Common;

using static OrderManagement.Application.Queries.GetOrderById.GetOrderByIdQuery;

namespace OrderManagement.Application.Queries.GetOrderById
{
    public class GetOrderByIdQuery : IRequest<Result<OrderDto>>
    {
        public Guid OrderId { get; set; }

        internal class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, Result<OrderDto>>
        {
            private readonly OrderDbContext _dbContext;

            public GetOrderByIdQueryHandler(OrderDbContext dbContext)
            {
                _dbContext = dbContext;
            }

            public async Task<Result<OrderDto>> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
            {
                try
                {
                    var order = await _dbContext.Orders
                        .Include(o => o.Items)
                        .AsNoTracking()
                        .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

                    if (order == null)
                    {
                        return Result<OrderDto>.Failure("Поръчката не е намерена");
                    }

                    var orderDto = new OrderDto
                    {
                        Id = order.Id,
                        OrderNumber = order.OrderNumber,
                        CustomerId = order.CustomerId,
                        OrderDate = order.OrderDate,
                        Status = order.Status.ToString(),
                        ShippingAddress = new AddressDto
                        {
                            Street = order.ShippingAddress.Street,
                            City = order.ShippingAddress.City,
                            PostalCode = order.ShippingAddress.PostalCode,
                            Country = order.ShippingAddress.Country
                        },
                        TotalAmount = order.TotalAmount.Amount,
                        Currency = order.TotalAmount.Currency,
                        ShippedDate = order.ShippedDate,
                        DeliveredDate = order.DeliveredDate,
                        CancellationReason = order.CancellationReason,
                        Items = order.Items.Select(i => new OrderItemDto
                        {
                            Id = i.Id,
                            ProductId = i.ProductId,
                            ProductName = i.ProductName,
                            UnitPrice = i.UnitPrice.Amount,
                            Quantity = i.Quantity,
                            Subtotal = i.Subtotal.Amount
                        }).ToList()
                    };

                    return Result<OrderDto>.Success(orderDto);
                }
                catch (Exception ex)
                {
                    return Result<OrderDto>.Failure($"Грешка при извличане на поръчка: {ex.Message}");
                }
            }
        }

        public class OrderDto
        {
            public Guid Id { get; set; }
            public string OrderNumber { get; set; }
            public Guid CustomerId { get; set; }
            public DateTime OrderDate { get; set; }
            public string Status { get; set; }
            public AddressDto ShippingAddress { get; set; }
            public decimal TotalAmount { get; set; }
            public string Currency { get; set; } = "BGN";
            public DateTime? ShippedDate { get; set; }
            public DateTime? DeliveredDate { get; set; }
            public string? CancellationReason { get; set; }
            public List<OrderItemDto> Items { get; set; } = new();
        }

        public class AddressDto
        {
            public string Street { get; set; }
            public string City { get; set; }
            public string PostalCode { get; set; }
            public string Country { get; set; }
        }

        public class OrderItemDto
        {
            public Guid Id { get; set; }
            public Guid ProductId { get; set; }
            public string ProductName { get; set; }
            public decimal UnitPrice { get; set; }
            public int Quantity { get; set; }
            public decimal Subtotal { get; set; }
        }
    }
}
