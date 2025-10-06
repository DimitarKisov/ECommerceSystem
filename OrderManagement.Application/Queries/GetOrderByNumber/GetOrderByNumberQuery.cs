using MediatR;
using OrderManagement.Application.Common;
using OrderManagement.Application.DTOs;

namespace OrderManagement.Application.Queries.GetOrderByNumber
{
    public class GetOrderByNumberQuery : IRequest<Result<OrderDto>>
    {
        public string OrderNumber { get; set; }

        internal class GetOrderByNumberQueryHandler : IRequestHandler<GetOrderByNumberQuery, Result<OrderDto>>
        {
            private readonly IOrderService _orderService;

            public GetOrderByNumberQueryHandler(IOrderService orderService)
            {
                _orderService = orderService;
            }

            public async Task<Result<OrderDto>> Handle(GetOrderByNumberQuery request, CancellationToken cancellationToken)
            {
                try
                {
                    var orderDto = await _orderService.GetOrderDtoByNumberAsync(request.OrderNumber, cancellationToken);

                    if (orderDto == null)
                    {
                        return Result<OrderDto>.Failure($"Поръчка с номер {request.OrderNumber} не е намерена");
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
