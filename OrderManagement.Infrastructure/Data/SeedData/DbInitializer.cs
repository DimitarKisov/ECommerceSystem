using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrderManagement.Infrastructure.Data.SeedData;

namespace OrderManagement.Infrastructure.Data
{
    /// <summary>
    /// Helper клас за инициализация на базата данни
    /// </summary>
    public static class DbInitializer
    {
        /// <summary>
        /// Прилага pending migrations и seed данни
        /// </summary>
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var services = scope.ServiceProvider;

            try
            {
                var context = services.GetRequiredService<OrderManagementDbContext>();
                var logger = services.GetRequiredService<ILogger<OrderManagementDbContext>>();

                logger.LogInformation("Проверка за pending migrations...");

                // Прилагаме pending migrations
                var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
                if (pendingMigrations.Any())
                {
                    logger.LogInformation(
                        "Намерени {Count} pending migrations. Прилагане...",
                        pendingMigrations.Count());

                    await context.Database.MigrateAsync();

                    logger.LogInformation("Migrations приложени успешно");
                }
                else
                {
                    logger.LogInformation("Няма pending migrations");
                }

                // Seed данни (само в Development)
                logger.LogInformation("Проверка дали е нужен seed на данни...");
                await OrderSeeder.SeedAsync(context, logger);
            }
            catch (Exception ex)
            {
                var logger = services.GetRequiredService<ILogger<OrderManagementDbContext>>();
                logger.LogError(ex, "Грешка при инициализация на базата данни");
                throw;
            }
        }

        /// <summary>
        /// Изтрива и пресъздава базата (САМО ЗА DEVELOPMENT!)
        /// </summary>
        public static async Task RecreateAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var services = scope.ServiceProvider;

            var context = services.GetRequiredService<OrderManagementDbContext>();
            var logger = services.GetRequiredService<ILogger<OrderManagementDbContext>>();

            logger.LogWarning("ВНИМАНИЕ: Изтриване на базата данни...");

            await context.Database.EnsureDeletedAsync();

            logger.LogInformation("Създаване на нова база данни...");

            await context.Database.MigrateAsync();

            logger.LogInformation("Seed на данни...");

            await OrderSeeder.SeedAsync(context, logger);

            logger.LogInformation("База данните е пресъздадена успешно");
        }
    }
}