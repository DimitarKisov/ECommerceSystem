using FluentValidation;

namespace OrderManagement.Application.Commands.ConfirmOrder
{
    public class ConfirmOrderCommandValidator : AbstractValidator<ConfirmOrderCommand>
    {
        public ConfirmOrderCommandValidator()
        {
            RuleFor(x => x.OrderId)
                .NotEmpty()
                .WithMessage("ID на поръчката е задължително");
        }
    }
}
