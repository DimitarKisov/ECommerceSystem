using MediatR;
using OrderManagement.Application.Common;

namespace OrderManagement.Application.Commands.DeliverOrder
{
    public class DeliverOrderCommand : IRequest<Result<bool>>
    {
        public Guid OrderId { get; set; }

        internal class DeliverOrderCommandHandler : IRequestHandler<DeliverOrderCommand, Result<bool>>
        {
            private readonly IOrderService _orderService;

            public DeliverOrderCommandHandler(IOrderService orderService)
            {
                _orderService = orderService;
            }

            public async Task<Result<bool>> Handle(DeliverOrderCommand request, CancellationToken cancellationToken)
            {
                try
                {
                    var order = await _orderService.GetByIdAsync(request.OrderId, cancellationToken);

                    if (order == null)
                    {
                        return Result<bool>.Failure("Поръчката не е намерена");
                    }

                    order.MarkAsDelivered();
                    await _orderService.SaveChangesAsync(cancellationToken);

                    return Result<bool>.Success(true);
                }
                catch (InvalidOperationException ex)
                {
                    return Result<bool>.Failure(ex.Message);
                }
                catch (Exception ex)
                {
                    return Result<bool>.Failure($"Грешка при доставка на поръчка: {ex.Message}");
                }
            }
        }
    }
}
