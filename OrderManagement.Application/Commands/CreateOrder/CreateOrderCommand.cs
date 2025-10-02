using MediatR;
using OrderManagement.Application.Common;
using OrderManagement.Domain.Entities;
using OrderManagement.Domain.ValueObjetcs;

namespace OrderManagement.Application.Commands.CreateOrder
{
    /// <summary>
    /// Команда за създаване на нова поръчка
    /// </summary>
    public record CreateOrderCommand : ICommand<Result<Guid>>
    {
        public Guid CustomerId { get; init; }
        public AddressDto ShippingAddress { get; init; }
        public List<OrderItemDto> Items { get; init; } = new();

        internal class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, Result<Guid>>
        {
            private readonly OrderDbContext _dbContext;

            public CreateOrderCommandHandler(OrderDbContext dbContext)
            {
                _dbContext = dbContext;
            }

            public async Task<Result<Guid>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
            {
                try
                {
                    // Създаваме адрес value object
                    var address = new Address(
                        request.ShippingAddress.Street,
                        request.ShippingAddress.City,
                        request.ShippingAddress.PostalCode,
                        request.ShippingAddress.Country
                    );

                    // Създаваме поръчката
                    var order = Order.Create(request.CustomerId, address);

                    // Добавяме продуктите
                    foreach (var item in request.Items)
                    {
                        var unitPrice = new Money(item.UnitPrice);
                        order.AddItem(item.ProductId, item.ProductName, unitPrice, item.Quantity);
                    }

                    // Записваме в базата
                    await _dbContext.Orders.AddAsync(order, cancellationToken);
                    await _dbContext.SaveChangesAsync(cancellationToken);

                    return Result<Guid>.Success(order.Id);
                }
                catch (Exception ex)
                {
                    return Result<Guid>.Failure($"Грешка при създаване на поръчка: {ex.Message}");
                }
            }
        }

        public record AddressDto
        {
            public string Street { get; init; }
            public string City { get; init; }
            public string PostalCode { get; init; }
            public string Country { get; init; }
        }

        public record OrderItemDto
        {
            public Guid ProductId { get; init; }
            public string ProductName { get; init; }
            public decimal UnitPrice { get; init; }
            public int Quantity { get; init; }
        }
    }
}
