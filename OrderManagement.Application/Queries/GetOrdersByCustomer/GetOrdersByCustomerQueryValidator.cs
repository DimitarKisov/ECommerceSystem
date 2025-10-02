using FluentValidation;

namespace OrderManagement.Application.Queries.GetOrdersByCustomer
{
    public class GetOrdersByCustomerQueryValidator : AbstractValidator<GetOrdersByCustomerQuery>
    {
        public GetOrdersByCustomerQueryValidator()
        {
            RuleFor(x => x.CustomerId)
                .NotEmpty()
                .WithMessage("ID на клиента е задължително");
        }
    }
}
