using MediatR;
using OrderManagement.Application.Common;

namespace OrderManagement.Application.Commands.CancelOrder
{
    public class CancelOrderCommand : IRequest<Result<bool>>
    {
        public Guid OrderId { get; set; }
        public string Reason { get; set; }

        internal class CancelOrderCommandHandler : IRequestHandler<CancelOrderCommand, Result<bool>>
        {
            private readonly OrderDbContext _dbContext;

            public CancelOrderCommandHandler(OrderDbContext dbContext)
            {
                _dbContext = dbContext;
            }

            public async Task<Result<bool>> Handle(CancelOrderCommand request, CancellationToken cancellationToken)
            {
                try
                {
                    var order = await _dbContext.Orders
                        .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

                    if (order == null)
                    {
                        return Result<bool>.Failure("Поръчката не е намерена");
                    }

                    order.Cancel(request.Reason);
                    await _dbContext.SaveChangesAsync(cancellationToken);

                    return Result<bool>.Success(true);
                }
                catch (InvalidOperationException ex)
                {
                    return Result<bool>.Failure(ex.Message);
                }
                catch (Exception ex)
                {
                    return Result<bool>.Failure($"Грешка при отмяна на поръчка: {ex.Message}");
                }
            }
        }
    }
}
