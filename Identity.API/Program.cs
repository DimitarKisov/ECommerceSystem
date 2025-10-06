using Identity.API.Data;
using Identity.API.Interfaces;
using Identity.API.Models;
using Identity.API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Конфигурираме Serilog за логване
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers();

// Database Context
builder.Services.AddDbContext<IdentityDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("IdentityDb"),
        b => b.MigrationsAssembly(typeof(IdentityDbContext).Assembly.FullName)));

// Identity Configuration
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Password settings
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 6;
    options.Password.RequiredUniqueChars = 1;

    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // User settings
    options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
    options.User.RequireUniqueEmail = true;

    // Email confirmation (можете да го включите ако имате email service)
    options.SignIn.RequireConfirmedEmail = false;
    options.SignIn.RequireConfirmedPhoneNumber = false;

    // Token providers
    options.Tokens.EmailConfirmationTokenProvider = TokenOptions.DefaultEmailProvider;
    options.Tokens.PasswordResetTokenProvider = TokenOptions.DefaultEmailProvider;
})
.AddEntityFrameworkStores<IdentityDbContext>()
.AddDefaultTokenProviders();

// JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey не е конфигуриран");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ClockSkew = TimeSpan.Zero
    };

    // Event handlers за JWT
    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            Log.Warning("JWT Authentication failed: {Error}", context.Exception.Message);
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            Log.Information("JWT Token validated for user: {UserId}",
                context.Principal?.Identity?.Name);
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "Identity API",
        Version = "v1",
        Description = "API за управление на потребители и автентикация в e-commerce система"
    });

    // JWT конфигурация за Swagger
    c.AddSecurityDefinition("Bearer", new()
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new()
    {
        {
            new()
            {
                Reference = new()
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Регистрираме наши services
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IIdentityService, IdentityService>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Identity API v1");
    });
}

// Global exception handler middleware
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/json";

        var error = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();
        if (error != null)
        {
            var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogError(error.Error, "Необработена грешка в Identity API");

            await context.Response.WriteAsJsonAsync(new
            {
                success = false,
                error = "Възникна грешка при обработката на заявката"
            });
        }
    });
});

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Health check endpoints
app.MapGet("/health", () => Results.Ok(new
{
    status = "healthy",
    service = "Identity.API",
    timestamp = DateTime.UtcNow
}))
.WithTags("Health")
.AllowAnonymous();

// Database initialization и seed данни
if (app.Environment.IsDevelopment())
{
    Log.Information("Initializing Identity database...");
    await InitializeDatabaseAsync(app.Services);
    Log.Information("Identity database initialized successfully");
}

Log.Information("Starting Identity API");

app.Run();

// ========================================
// Database Initialization Methods
// ========================================

/// <summary>
/// Инициализира базата данни и seed данни
/// </summary>
static async Task InitializeDatabaseAsync(IServiceProvider serviceProvider)
{
    using var scope = serviceProvider.CreateScope();
    var services = scope.ServiceProvider;

    try
    {
        var context = services.GetRequiredService<IdentityDbContext>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var logger = services.GetRequiredService<ILogger<Program>>();

        // Прилагаме migrations
        logger.LogInformation("Проверка за pending migrations...");
        var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
        if (pendingMigrations.Any())
        {
            logger.LogInformation("Намерени {Count} pending migrations. Прилагане...", pendingMigrations.Count());
            await context.Database.MigrateAsync();
            logger.LogInformation("Migrations приложени успешно");
        }

        // Seed ролите
        await SeedRolesAsync(roleManager, logger);

        // Seed администраторския потребител
        await SeedAdminUserAsync(userManager, logger);

        // Seed тестови потребители
        await SeedTestUsersAsync(userManager, logger);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Грешка при инициализация на Identity базата данни");
        throw;
    }
}

/// <summary>
/// Seed на системните роли
/// </summary>
static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager, Microsoft.Extensions.Logging.ILogger logger)
{
    var roles = new[] { "Administrator", "Customer", "Employee" };

    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
            logger.LogInformation("Създадена роля: {RoleName}", role);
        }
    }
}

/// <summary>
/// Seed на администраторския потребител
/// </summary>
static async Task SeedAdminUserAsync(UserManager<ApplicationUser> userManager, Microsoft.Extensions.Logging.ILogger logger)
{
    var adminEmail = "admin@ecommerce.bg";

    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    if (adminUser == null)
    {
        adminUser = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            FirstName = "System",
            LastName = "Administrator",
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var result = await userManager.CreateAsync(adminUser, "Admin123!");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, "Administrator");
            logger.LogInformation("Създаден администраторски потребител: {Email}", adminEmail);
        }
        else
        {
            logger.LogError("Грешка при създаване на администраторския потребител: {Errors}",
                string.Join(", ", result.Errors.Select(e => e.Description)));
        }
    }
}

/// <summary>
/// Seed на тестови потребители за development
/// </summary>
static async Task SeedTestUsersAsync(UserManager<ApplicationUser> userManager, Microsoft.Extensions.Logging.ILogger logger)
{
    var testUsers = new[]
    {
        new { Email = "customer1@test.bg", FirstName = "Иван", LastName = "Петров", Role = "Customer" },
        new { Email = "customer2@test.bg", FirstName = "Мария", LastName = "Георгиева", Role = "Customer" },
        new { Email = "employee@test.bg", FirstName = "Петър", LastName = "Стоянов", Role = "Employee" }
    };

    foreach (var userData in testUsers)
    {
        var user = await userManager.FindByEmailAsync(userData.Email);
        if (user == null)
        {
            user = new ApplicationUser
            {
                UserName = userData.Email,
                Email = userData.Email,
                FirstName = userData.FirstName,
                LastName = userData.LastName,
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(user, "Test123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, userData.Role);
                logger.LogInformation("Създаден тестов потребител: {Email} с роля {Role}",
                    userData.Email, userData.Role);
            }
            else
            {
                logger.LogError("Грешка при създаване на тестов потребител {Email}: {Errors}",
                    userData.Email, string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }
    }
}