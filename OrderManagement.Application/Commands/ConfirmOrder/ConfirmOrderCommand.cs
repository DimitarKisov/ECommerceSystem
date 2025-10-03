using MediatR;
using OrderManagement.Application.Common;

namespace OrderManagement.Application.Commands.ConfirmOrder
{
    /// <summary>
    /// Команда за потвърждаване на поръчка
    /// </summary>
    public class ConfirmOrderCommand : IRequest<Result<bool>>
    {
        public Guid OrderId { get; set; }

        internal class ConfirmOrderCommandHandler : IRequestHandler<ConfirmOrderCommand, Result<bool>>
        {
            private readonly IOrderService _orderService;

            public ConfirmOrderCommandHandler(IOrderService orderService)
            {
                _orderService = orderService;
            }

            public async Task<Result<bool>> Handle(ConfirmOrderCommand request, CancellationToken cancellationToken)
            {
                try
                {
                    var order = await _orderService.GetByIdWithItemsAsync(request.OrderId, cancellationToken);

                    if (order == null)
                    {
                        return Result<bool>.Failure("Поръчката не е намерена");
                    }

                    order.Confirm();
                    await _orderService.SaveChangesAsync(cancellationToken);

                    return Result<bool>.Success(true);
                }
                catch (InvalidOperationException ex)
                {
                    return Result<bool>.Failure(ex.Message);
                }
                catch (Exception ex)
                {
                    return Result<bool>.Failure($"Грешка при потвърждаване на поръчка: {ex.Message}");
                }
            }
        }
    }
}
