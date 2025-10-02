using MediatR;

namespace OrderManagement.Application.Common
{
    /// <summary>
    /// Базов интерфейс за queries (read operations)
    /// </summary>
    public interface IQuery<out TResponse> : IRequest<TResponse>
    {
    }
}
