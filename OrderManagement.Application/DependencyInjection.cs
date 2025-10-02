using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using OrderManagement.Application.Behaviors;
using System.Reflection;

namespace OrderManagement.Application
{
    /// <summary>
    /// Extension methods за регистрация на Application слоя
    /// </summary>
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            var assembly = Assembly.GetExecutingAssembly();

            // Регистрираме MediatR
            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(assembly);

                // Добавяме pipeline behaviors
                cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
                cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
            });

            // Регистрираме всички validators от FluentValidation
            services.AddValidatorsFromAssembly(assembly);

            return services;
        }
    }
}
