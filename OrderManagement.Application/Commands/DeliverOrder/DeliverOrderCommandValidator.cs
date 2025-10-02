using FluentValidation;

namespace OrderManagement.Application.Commands.DeliverOrder
{
    public class DeliverOrderCommandValidator : AbstractValidator<DeliverOrderCommand>
    {
        public DeliverOrderCommandValidator()
        {
            RuleFor(x => x.OrderId)
                .NotEmpty()
                .WithMessage("ID на поръчката е задължително");
        }
    }
}
