using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrderManagement.Application.Common;
using OrderManagement.Infrastructure.Data;
using OrderManagement.Infrastructure.Messaging;
using OrderManagement.Infrastructure.Services;
using System.Reflection;

namespace OrderManagement.Infrastructure
{
    /// <summary>
    /// Extension methods за регистрация на Infrastructure слоя
    /// </summary>
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Регистрираме DbContext
            services.AddDbContext<OrderManagementDbContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("OrderManagementDb"),
                    b => b.MigrationsAssembly(typeof(OrderManagementDbContext).Assembly.FullName)));

            // Регистрираме Repository
            services.AddScoped<IOrderService, OrderService>();

            // Регистрираме MessageBus за RabbitMQ
            services.AddSingleton<IMessageBus, RabbitMqMessageBus>();

            // Регистрираме MediatR за domain event handlers
            services.AddMediatR(cfg =>
                cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

            return services;
        }
    }
}