using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Moq;
using OrderManagement.Infrastructure.Data;
using OrderManagement.Infrastructure.Messaging;

namespace OrderManagement.Tests.Integration.Common
{
    /// <summary>
    /// Custom WebApplicationFactory за integration tests
    /// Използва In-Memory database и mock-ва RabbitMQ
    /// </summary>
    public class OrderManagementApiFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // Премахваме реалната SQL Server database
                services.RemoveAll<DbContextOptions<OrderManagementDbContext>>();
                services.RemoveAll<OrderManagementDbContext>();

                // Добавяме In-Memory database за тестове
                services.AddDbContext<OrderManagementDbContext>(options =>
                {
                    options.UseInMemoryDatabase("TestDatabase");
                });

                // Mock-ваме RabbitMQ MessageBus
                services.RemoveAll<IMessageBus>();
                var messageBusMock = new Mock<IMessageBus>();
                messageBusMock
                    .Setup(x => x.PublishAsync(
                        It.IsAny<object>(),
                        It.IsAny<string>(),
                        It.IsAny<CancellationToken>()))
                    .Returns(Task.CompletedTask);
                services.AddSingleton(messageBusMock.Object);

                // Build service provider и seed database
                var sp = services.BuildServiceProvider();

                using var scope = sp.CreateScope();
                var scopedServices = scope.ServiceProvider;
                var db = scopedServices.GetRequiredService<OrderManagementDbContext>();

                // Ensure database е създадена
                db.Database.EnsureCreated();
            });

            builder.UseEnvironment("Test");
        }

        /// <summary>
        /// Създава нов scope за достъп до services
        /// </summary>
        public IServiceScope CreateScope()
        {
            return Services.CreateScope();
        }

        /// <summary>
        /// Изчиства database след всеки тест
        /// </summary>
        public void ResetDatabase()
        {
            using var scope = CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<OrderManagementDbContext>();
            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();
        }
    }
}