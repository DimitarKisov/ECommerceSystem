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
            private readonly OrderDbContext _dbContext;

            public ConfirmOrderCommandHandler(OrderDbContext dbContext)
            {
                _dbContext = dbContext;
            }

            public async Task<Result<bool>> Handle(ConfirmOrderCommand request, CancellationToken cancellationToken)
            {
                try
                {
                    var order = await _dbContext.Orders
                        .Include(o => o.Items)
                        .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

                    if (order == null)
                    {
                        return Result<bool>.Failure("Поръчката не е намерена");
                    }

                    order.Confirm();
                    await _dbContext.SaveChangesAsync(cancellationToken);

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
