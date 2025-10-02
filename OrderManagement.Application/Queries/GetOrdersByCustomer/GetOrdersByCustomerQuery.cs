using MediatR;
using OrderManagement.Application.Common;

using static OrderManagement.Application.Queries.GetOrdersByCustomer.GetOrdersByCustomerQuery;

namespace OrderManagement.Application.Queries.GetOrdersByCustomer
{
    public class GetOrdersByCustomerQuery : IRequest<Result<List<OrderSummaryDto>>>
    {
        public Guid CustomerId { get; set; }

        internal class GetOrdersByCustomerQueryHandler : IRequestHandler<GetOrdersByCustomerQuery, Result<List<OrderSummaryDto>>>
        {
            private readonly OrderDbContext _dbContext;

            public GetOrdersByCustomerQueryHandler(OrderDbContext dbContext)
            {
                _dbContext = dbContext;
            }

            public async Task<Result<List<OrderSummaryDto>>> Handle(GetOrdersByCustomerQuery request, CancellationToken cancellationToken)
            {
                try
                {
                    var summaries = await _dbContext.Orders
                        .Include(o => o.Items)
                        .AsNoTracking()
                        .Where(o => o.CustomerId == request.CustomerId)
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

                    return Result<List<OrderSummaryDto>>.Success(summaries);
                }
                catch (Exception ex)
                {
                    return Result<List<OrderSummaryDto>>.Failure($"Грешка при извличане на поръчки: {ex.Message}");
                }
            }
        }

        public class OrderSummaryDto
        {
            public Guid Id { get; set; }
            public string OrderNumber { get; set; }
            public DateTime OrderDate { get; set; }
            public string Status { get; set; }
            public decimal TotalAmount { get; set; }
            public int ItemsCount { get; set; }
        }
    }
}
