using FluentValidation;

namespace OrderManagement.Application.Commands.CancelOrder
{
    public class CancelOrderValidator : AbstractValidator<CancelOrderCommand>
    {
        public CancelOrderValidator()
        {
            RuleFor(x => x.OrderId)
                .NotEmpty()
                .WithMessage("ID на поръчката е задължително");

            RuleFor(x => x.Reason)
                .NotEmpty()
                .WithMessage("Причината за отмяна е задължителна")
                .MaximumLength(500);
        }
    }
}
