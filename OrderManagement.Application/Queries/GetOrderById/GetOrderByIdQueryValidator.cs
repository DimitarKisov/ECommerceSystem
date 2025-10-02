using FluentValidation;

namespace OrderManagement.Application.Queries.GetOrderById
{
    public class GetOrderByIdQueryValidator : AbstractValidator<GetOrderByIdQuery>
    {
        public GetOrderByIdQueryValidator()
        {
            RuleFor(x => x.OrderId)
                .NotEmpty()
                .WithMessage("ID на поръчката е задължително");
        }
    }
}
