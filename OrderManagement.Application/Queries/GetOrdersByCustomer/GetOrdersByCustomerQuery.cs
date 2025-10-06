using MediatR;
using OrderManagement.Application.Common;
using OrderManagement.Application.DTOs;

namespace OrderManagement.Application.Queries.GetOrdersByCustomer
{
    public class GetOrdersByCustomerQuery : IRequest<Result<List<OrderSummaryDto>>>
    {
        public Guid CustomerId { get; set; }

        internal class GetOrdersByCustomerQueryHandler : IRequestHandler<GetOrdersByCustomerQuery, Result<List<OrderSummaryDto>>>
        {
            private readonly IOrderService _orderService;

            public GetOrdersByCustomerQueryHandler(IOrderService orderService)
            {
                _orderService = orderService;
            }

            public async Task<Result<List<OrderSummaryDto>>> Handle(
                GetOrdersByCustomerQuery request,
                CancellationToken cancellationToken)
            {
                try
                {
                    var summaries = await _orderService.GetOrdersByCustomerIdAsync(
                        request.CustomerId,
                        cancellationToken);

                    return Result<List<OrderSummaryDto>>.Success(summaries);
                }
                catch (Exception ex)
                {
                    return Result<List<OrderSummaryDto>>.Failure($"Грешка при извличане на поръчки: {ex.Message}");
                }
            }
        }
    }
}
