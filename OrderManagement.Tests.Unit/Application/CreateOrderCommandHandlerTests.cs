using FluentValidation;

namespace OrderManagement.Application.Commands.CreateOrder
{
    /// <summary>
    /// Валидатор за CreateOrderCommand
    /// </summary>
    public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
    {
        public CreateOrderCommandValidator()
        {
            RuleFor(x => x.CustomerId)
                .NotEmpty()
                .WithMessage("ID на клиента е задължително");

            RuleFor(x => x.ShippingAddress)
                .NotNull()
                .WithMessage("Адресът за доставка е задължителен");

            // ВАЖНО: When check за да избегнем NullReferenceException
            When(x => x.ShippingAddress != null, () =>
            {
                RuleFor(x => x.ShippingAddress.Street)
                    .NotEmpty()
                    .WithMessage("Улицата е задължителна")
                    .MaximumLength(200);

                RuleFor(x => x.ShippingAddress.City)
                    .NotEmpty()
                    .WithMessage("Градът е задължителен")
                    .MaximumLength(100);

                RuleFor(x => x.ShippingAddress.PostalCode)
                    .NotEmpty()
                    .WithMessage("Пощенският код е задължителен")
                    .MaximumLength(20);

                RuleFor(x => x.ShippingAddress.Country)
                    .NotEmpty()
                    .WithMessage("Държавата е задължителна")
                    .MaximumLength(100);
            });

            RuleFor(x => x.Items)
                .NotEmpty()
                .WithMessage("Поръчката трябва да има поне един продукт");

            RuleForEach(x => x.Items).ChildRules(item =>
            {
                item.RuleFor(x => x.ProductId)
                    .NotEmpty()
                    .WithMessage("ID на продукта е задължително");

                item.RuleFor(x => x.ProductName)
                    .NotEmpty()
                    .WithMessage("Името на продукта е задължително")
                    .MaximumLength(200);

                item.RuleFor(x => x.UnitPrice)
                    .GreaterThan(0)
                    .WithMessage("Цената трябва да е положителна");

                item.RuleFor(x => x.Quantity)
                    .GreaterThan(0)
                    .WithMessage("Количеството трябва да е положително число");
            });
        }
    }
}