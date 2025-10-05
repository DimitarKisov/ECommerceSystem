using MediatR;
using Microsoft.AspNetCore.Mvc;
using OrderManagement.Application.Common;

namespace OrderManagement.API.Controllers
{
    /// <summary>
    /// Базов API контролер с общи функционалности
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public abstract class BaseApiController : ControllerBase
    {
        private IMediator _mediator;

        /// <summary>
        /// Lazy initialization на MediatR
        /// </summary>
        protected IMediator Mediator =>
            _mediator ??= HttpContext.RequestServices.GetService<IMediator>();

        /// <summary>
        /// Помощен метод за обработка на Result pattern
        /// Конвертира Result в подходящ HTTP response
        /// </summary>
        protected IActionResult HandleResult<T>(Result<T> result)
        {
            if (result.IsSuccess)
            {
                return Ok(new
                {
                    success = true,
                    data = result.Data
                });
            }

            // Ако има validation errors
            if (result.ValidationErrors.Any())
            {
                return BadRequest(new
                {
                    success = false,
                    errors = result.ValidationErrors
                });
            }

            // Ако има обща грешка
            return BadRequest(new
            {
                success = false,
                error = result.Error
            });
        }

        /// <summary>
        /// Помощен метод за обработка на Result без данни
        /// </summary>
        protected IActionResult HandleResult(Result<bool> result)
        {
            if (result.IsSuccess)
            {
                return Ok(new
                {
                    success = true,
                    message = "Операцията е изпълнена успешно"
                });
            }

            if (result.ValidationErrors.Any())
            {
                return BadRequest(new
                {
                    success = false,
                    errors = result.ValidationErrors
                });
            }

            return BadRequest(new
            {
                success = false,
                error = result.Error
            });
        }
    }
}