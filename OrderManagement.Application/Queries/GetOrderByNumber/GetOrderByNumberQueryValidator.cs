using FluentValidation;

namespace OrderManagement.Application.Queries.GetOrderByNumber
{
    public class GetOrderByNumberQueryValidator : AbstractValidator<GetOrderByNumberQuery>
    {
        public GetOrderByNumberQueryValidator()
        {
            RuleFor(x => x.OrderNumber)
                .NotEmpty()
                .WithMessage("Номерът на поръчката е задължителен")
                .MaximumLength(50)
                .WithMessage("Номерът на поръчката не може да бъде по-дълъг от 50 символа");
        }
    }
}
