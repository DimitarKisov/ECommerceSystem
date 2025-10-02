using MediatR;

namespace OrderManagement.Application.Common
{
    /// <summary>
    /// Базов интерфейс за команди (write operations)
    /// </summary>
    public interface ICommand<out TResponse> : IRequest<TResponse>
    {
    }

    /// <summary>
    /// Базов интерфейс за команди без резултат
    /// </summary>
    public interface ICommand : IRequest
    {
    }
}
