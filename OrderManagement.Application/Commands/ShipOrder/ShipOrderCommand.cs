using MediatR;
using OrderManagement.Application.Common;

namespace OrderManagement.Application.Commands.ShipOrder
{
    public class ShipOrderCommand : IRequest<Result<bool>>
    {
        public Guid OrderId { get; set; }

        internal class ShipOrderCommandHandler : IRequestHandler<ShipOrderCommand, Result<bool>>
        {
            private readonly IOrderService _orderService;

            public ShipOrderCommandHandler(IOrderService orderService)
            {
                _orderService = orderService;
            }

            public async Task<Result<bool>> Handle(ShipOrderCommand request, CancellationToken cancellationToken)
            {
                try
                {
                    var order = await _orderService.GetByIdAsync(request.OrderId, cancellationToken);

                    if (order == null)
                    {
                        return Result<bool>.Failure("Поръчката не е намерена");
                    }

                    order.MarkAsShipped();
                    await _orderService.SaveChangesAsync(cancellationToken);

                    return Result<bool>.Success(true);
                }
                catch (InvalidOperationException ex)
                {
                    return Result<bool>.Failure(ex.Message);
                }
                catch (Exception ex)
                {
                    return Result<bool>.Failure($"Грешка при изпращане на поръчка: {ex.Message}");
                }
            }
        }
    }
}
