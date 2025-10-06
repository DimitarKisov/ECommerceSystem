using MediatR;
using OrderManagement.Application.Common;
using OrderManagement.Application.DTOs;

namespace OrderManagement.Application.Queries.GetOrderById
{
    public class GetOrderByIdQuery : IRequest<Result<OrderDto>>
    {
        public Guid OrderId { get; set; }

        internal class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, Result<OrderDto>>
        {
            private readonly IOrderService _orderService;

            public GetOrderByIdQueryHandler(IOrderService orderService)
            {
                _orderService = orderService;
            }

            public async Task<Result<OrderDto>> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
            {
                try
                {
                    var orderDto = await _orderService.GetOrderDtoByIdAsync(request.OrderId, cancellationToken);

                    if (orderDto == null)
                    {
                        return Result<OrderDto>.Failure("Поръчката не е намерена");
                    }

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
