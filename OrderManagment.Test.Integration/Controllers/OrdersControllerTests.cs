using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using OrderManagement.Application.Commands.CreateOrder;
using OrderManagement.Infrastructure.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using OrderManagement.Tests.Integration.Common;
using Xunit;

namespace OrderManagement.Tests.Integration.Controllers
{
    /// <summary>
    /// Integration tests за OrdersController
    /// Тестваме реални HTTP requests към API endpoints
    /// </summary>
    public class OrdersControllerTests : IClassFixture<OrderManagementApiFactory>
    {
        private readonly OrderManagementApiFactory _factory;
        private readonly HttpClient _client;

        public OrdersControllerTests(OrderManagementApiFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
            
            // Добавяме JWT token към всички requests
            var token = GenerateJwtToken();
            _client.DefaultRequestHeaders.Authorization = 
                new AuthenticationHeaderValue("Bearer", token);
        }

        [Fact]
        public async Task CreateOrder_WithValidData_ShouldReturn200AndCreateOrder()
        {
            // Arrange
            _factory.ResetDatabase();
            
            var command = new CreateOrderCommand
            {
                CustomerId = Guid.NewGuid(),
                ShippingAddress = new CreateOrderCommand.AddressDto
                {
                    Street = "ул. Витоша 100",
                    City = "София",
                    PostalCode = "1000",
                    Country = "България"
                },
                Items = new List<CreateOrderCommand.OrderItemDto>
                {
                    new()
                    {
                        ProductId = Guid.NewGuid(),
                        ProductName = "Test Product",
                        UnitPrice = 100m,
                        Quantity = 2
                    }
                }
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/orders", command);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var content = await response.Content.ReadFromJsonAsync<ApiResponse>();
            content.Should().NotBeNull();
            content!.Success.Should().BeTrue();
            content.Data.Should().NotBeNull();

            // Проверяваме в базата
            using var scope = _factory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<OrderManagementDbContext>();
            var order = await db.Orders.FindAsync(content.Data!.Value);
            order.Should().NotBeNull();
            order!.CustomerId.Should().Be(command.CustomerId);
        }

        [Fact]
        public async Task CreateOrder_WithInvalidData_ShouldReturn400WithValidationErrors()
        {
            // Arrange
            var command = new CreateOrderCommand
            {
                CustomerId = Guid.Empty, // Invalid!
                ShippingAddress = new CreateOrderCommand.AddressDto
                {
                    Street = "", // Invalid!
                    City = "София",
                    PostalCode = "1000",
                    Country = "България"
                },
                Items = new List<CreateOrderCommand.OrderItemDto>()  // Invalid - empty!
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/orders", command);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var content = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
            content.Should().NotBeNull();
            content!.Success.Should().BeFalse();
            content.Errors.Should().NotBeEmpty();
            content.Errors.Should().HaveCountGreaterThan(2);
        }

        [Fact]
        public async Task GetOrderById_WhenOrderExists_ShouldReturn200WithOrder()
        {
            // Arrange
            _factory.ResetDatabase();
            
            var orderId = await CreateTestOrder();

            // Act
            var response = await _client.GetAsync($"/api/orders/{orderId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var content = await response.Content.ReadFromJsonAsync<ApiResponse>();
            content.Should().NotBeNull();
            content!.Success.Should().BeTrue();
            content.Data.Should().NotBeNull();
        }

        [Fact]
        public async Task GetOrderById_WhenOrderDoesNotExist_ShouldReturn400()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();

            // Act
            var response = await _client.GetAsync($"/api/orders/{nonExistentId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task ConfirmOrder_WhenOrderExists_ShouldReturn200AndConfirmOrder()
        {
            // Arrange
            _factory.ResetDatabase();
            var orderId = await CreateTestOrder();

            // Act
            var response = await _client.PostAsync($"/api/orders/{orderId}/confirm", null);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            // Проверяваме в базата че статусът е Confirmed
            using var scope = _factory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<OrderManagementDbContext>();
            var order = await db.Orders.FindAsync(orderId);
            order.Should().NotBeNull();
            order!.Status.ToString().Should().Be("Confirmed");
        }

        [Fact]
        public async Task CancelOrder_WithValidReason_ShouldReturn200AndCancelOrder()
        {
            // Arrange
            _factory.ResetDatabase();
            var orderId = await CreateTestOrder();
            await _client.PostAsync($"/api/orders/{orderId}/confirm", null);

            var cancelRequest = new { Reason = "Клиентът промени решението" };

            // Act
            var response = await _client.PostAsJsonAsync(
                $"/api/orders/{orderId}/cancel", 
                cancelRequest);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            // Проверяваме в базата
            using var scope = _factory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<OrderManagementDbContext>();
            var order = await db.Orders.FindAsync(orderId);
            order.Should().NotBeNull();
            order!.Status.ToString().Should().Be("Cancelled");
        }

        [Fact]
        public async Task GetOrdersByCustomer_ShouldReturnCustomerOrders()
        {
            // Arrange
            _factory.ResetDatabase();
            var customerId = Guid.NewGuid();
            
            // Създаваме 3 поръчки за този клиент
            await CreateTestOrder(customerId);
            await CreateTestOrder(customerId);
            await CreateTestOrder(customerId);
            
            // И една поръчка за друг клиент
            await CreateTestOrder();

            // Act
            var response = await _client.GetAsync($"/api/orders/customer/{customerId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var content = await response.Content.ReadFromJsonAsync<ApiResponse>();
            content.Should().NotBeNull();
            content!.Success.Should().BeTrue();
            content.Data.Should().NotBeNull();

            // Трябва да върне само 3-те поръчки на този клиент
            var orders = System.Text.Json.JsonSerializer.Deserialize<List<object>>(
                content.Data!.ToString()!);
            orders.Should().HaveCount(3);
        }

        [Fact]
        public async Task Requests_WithoutAuthentication_ShouldReturn401()
        {
            // Arrange
            var clientWithoutAuth = _factory.CreateClient();
            // Не добавяме Authorization header

            // Act
            var response = await clientWithoutAuth.GetAsync("/api/orders/customer/" + Guid.NewGuid());

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        // ========================================
        // Helper Methods
        // ========================================

        private async Task<Guid> CreateTestOrder(Guid? customerId = null)
        {
            var command = new CreateOrderCommand
            {
                CustomerId = customerId ?? Guid.NewGuid(),
                ShippingAddress = new CreateOrderCommand.AddressDto
                {
                    Street = "ул. Витоша 100",
                    City = "София",
                    PostalCode = "1000",
                    Country = "България"
                },
                Items = new List<CreateOrderCommand.OrderItemDto>
                {
                    new()
                    {
                        ProductId = Guid.NewGuid(),
                        ProductName = "Test Product",
                        UnitPrice = 100m,
                        Quantity = 2
                    }
                }
            };

            var response = await _client.PostAsJsonAsync("/api/orders", command);
            var content = await response.Content.ReadFromJsonAsync<ApiResponse>();
            return content!.Data!.Value.GetGuid();
        }

        private string GenerateJwtToken()
        {
            var secretKey = "YourSuperSecretKeyThatIsAtLeast32CharactersLong!";
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Name, "Test User")
            };

            var token = new JwtSecurityToken(
                issuer: "OrderManagementAPI",
                audience: "ECommerceClients",
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // Response models за deserialization
        private record ApiResponse
        {
            public bool Success { get; init; }
            public System.Text.Json.JsonElement? Data { get; init; }
        }

        private record ApiErrorResponse
        {
            public bool Success { get; init; }
            public List<string> Errors { get; init; } = new();
        }
    }
}