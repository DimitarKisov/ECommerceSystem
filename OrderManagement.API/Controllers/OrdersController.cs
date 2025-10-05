using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderManagement.Application.Commands.CancelOrder;
using OrderManagement.Application.Commands.ConfirmOrder;
using OrderManagement.Application.Commands.CreateOrder;
using OrderManagement.Application.Commands.DeliverOrder;
using OrderManagement.Application.Commands.ShipOrder;
using OrderManagement.Application.Queries.GetOrderById;
using OrderManagement.Application.Queries.GetOrderByNumber;
using OrderManagement.Application.Queries.GetOrdersByCustomer;

namespace OrderManagement.API.Controllers
{
    /// <summary>
    /// API контролер за управление на поръчки
    /// </summary>
    [Authorize]
    public class OrdersController : BaseApiController
    {
        /// <summary>
        /// Създава нова поръчка
        /// POST: api/orders
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderCommand command)
        {
            var result = await Mediator.Send(command);
            return HandleResult(result);
        }

        /// <summary>
        /// Получава поръчка по ID
        /// GET: api/orders/{id}
        /// </summary>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetOrderById(Guid id)
        {
            var query = new GetOrderByIdQuery { OrderId = id };
            var result = await Mediator.Send(query);
            return HandleResult(result);
        }

        /// <summary>
        /// Получава поръчка по номер
        /// GET: api/orders/by-number/{orderNumber}
        /// </summary>
        [HttpGet("by-number/{orderNumber}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetOrderByNumber(string orderNumber)
        {
            var query = new GetOrderByNumberQuery { OrderNumber = orderNumber };
            var result = await Mediator.Send(query);
            return HandleResult(result);
        }

        /// <summary>
        /// Получава всички поръчки на клиент
        /// GET: api/orders/customer/{customerId}
        /// </summary>
        [HttpGet("customer/{customerId:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetOrdersByCustomer(Guid customerId)
        {
            var query = new GetOrdersByCustomerQuery { CustomerId = customerId };
            var result = await Mediator.Send(query);
            return HandleResult(result);
        }

        /// <summary>
        /// Потвърждава поръчка
        /// POST: api/orders/{id}/confirm
        /// </summary>
        [HttpPost("{id:guid}/confirm")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ConfirmOrder(Guid id)
        {
            var command = new ConfirmOrderCommand { OrderId = id };
            var result = await Mediator.Send(command);
            return HandleResult(result);
        }

        /// <summary>
        /// Отменя поръчка
        /// POST: api/orders/{id}/cancel
        /// </summary>
        [HttpPost("{id:guid}/cancel")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CancelOrder(Guid id, [FromBody] CancelOrderRequest request)
        {
            var command = new CancelOrderCommand
            {
                OrderId = id,
                Reason = request.Reason
            };
            var result = await Mediator.Send(command);
            return HandleResult(result);
        }

        /// <summary>
        /// Маркира поръчка като изпратена
        /// POST: api/orders/{id}/ship
        /// </summary>
        [HttpPost("{id:guid}/ship")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ShipOrder(Guid id)
        {
            var command = new ShipOrderCommand { OrderId = id };
            var result = await Mediator.Send(command);
            return HandleResult(result);
        }

        /// <summary>
        /// Маркира поръчка като доставена
        /// POST: api/orders/{id}/deliver
        /// </summary>
        [HttpPost("{id:guid}/deliver")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeliverOrder(Guid id)
        {
            var command = new DeliverOrderCommand { OrderId = id };
            var result = await Mediator.Send(command);
            return HandleResult(result);
        }

        /// <summary>
        /// Request model за отмяна на поръчка
        /// </summary>
        public record CancelOrderRequest
        {
            public string Reason { get; init; }
        }
    }
}