using FluentValidation;

namespace OrderManagement.Application.Commands.ShipOrder
{
    public class ShipOrderCommandValidator : AbstractValidator<ShipOrderCommand>
    {
        public ShipOrderCommandValidator()
        {
            RuleFor(x => x.OrderId)
                .NotEmpty()
                .WithMessage("ID на поръчката е задължително");
        }
    }
}
