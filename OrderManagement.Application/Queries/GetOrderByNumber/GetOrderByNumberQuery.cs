using MediatR;
using OrderManagement.Application.Common;

using static OrderManagement.Application.Queries.GetOrderById.GetOrderByIdQuery;

namespace OrderManagement.Application.Queries.GetOrderByNumber
{
    public class GetOrderByNumberQuery : IRequest<Result<OrderDto>>
    {
        public string OrderNumber { get; set; }

        internal class GetOrderByNumberQueryHandler : IRequestHandler<GetOrderByNumberQuery, Result<OrderDto>>
        {
            private readonly OrderDbContext _dbContext;

            public GetOrderByNumberQueryHandler(OrderDbContext dbContext)
            {
                _dbContext = dbContext;
            }

            public async Task<Result<OrderDto>> Handle(GetOrderByNumberQuery request, CancellationToken cancellationToken)
            {
                try
                {
                    var order = await _dbContext.Orders
                        .Include(o => o.Items)
                        .AsNoTracking()
                        .FirstOrDefaultAsync(o => o.OrderNumber == request.OrderNumber, cancellationToken);

                    if (order == null)
                    {
                        return Result<OrderDto>.Failure($"Поръчка с номер {request.OrderNumber} не е намерена");
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
    }
}
