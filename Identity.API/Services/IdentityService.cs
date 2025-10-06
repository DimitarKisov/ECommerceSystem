using Identity.API.DTOs;
using Identity.API.Interfaces;
using Identity.API.Models;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace Identity.API.Services
{
    public class IdentityService : IIdentityService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IJwtService _jwtService;
        private readonly ILogger<IdentityService> _logger;
        private readonly IConfiguration _configuration;

        public IdentityService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            IJwtService jwtService,
            ILogger<IdentityService> logger,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _jwtService = jwtService;
            _logger = logger;
            _configuration = configuration;
        }

        /// <summary>
        /// Смяна на парола
        /// </summary>
        public async Task<ApiResponse<bool>> ChangePasswordAsync(string userId, ChangePasswordRequest request)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return ApiResponse<bool>.ErrorResult("Потребителят не е намерен");
                }

                var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
                if (!result.Succeeded)
                {
                    var errors = result.Errors.Select(e => e.Description).ToList();
                    return ApiResponse<bool>.ErrorResult(errors);
                }

                // Инвалидираме всички refresh токени за security
                user.RefreshToken = null;
                user.RefreshTokenExpiryTime = null;
                user.UpdatedAt = DateTime.UtcNow;
                await _userManager.UpdateAsync(user);

                _logger.LogInformation("Password changed for user: {UserId}", userId);

                return ApiResponse<bool>.SuccessResult(true, "Паролата е сменена успешно");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password for user: {UserId}", userId);
                return ApiResponse<bool>.ErrorResult("Възникна грешка при смяната на паролата");
            }
        }

        /// <summary>
        /// Потвърждава email адрес
        /// </summary>
        public async Task<ApiResponse<bool>> ConfirmEmailAsync(string userId, string token)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return ApiResponse<bool>.ErrorResult("Невалиден потребител");
                }

                var result = await _userManager.ConfirmEmailAsync(user, token);
                if (!result.Succeeded)
                {
                    var errors = result.Errors.Select(e => e.Description).ToList();
                    return ApiResponse<bool>.ErrorResult(errors);
                }

                _logger.LogInformation("Email confirmed for user: {UserId}", userId);

                return ApiResponse<bool>.SuccessResult(true, "Email адресът е потвърден успешно");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error confirming email for user: {UserId}", userId);
                return ApiResponse<bool>.ErrorResult("Възникна грешка при потвърждаването на email адреса");
            }
        }

        /// <summary>
        /// Забравена парола - изпраща reset token
        /// </summary>
        public async Task<ApiResponse<bool>> ForgotPasswordAsync(ForgotPasswordRequest request)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(request.Email);
                if (user == null)
                {
                    // Не казваме на клиента че потребителят не съществува (security)
                    return ApiResponse<bool>.SuccessResult(true, "Ако email адресът съществува, ще получите инструкции за възстановяване");
                }

                var token = await _userManager.GeneratePasswordResetTokenAsync(user);

                // TODO: Изпратете email с reset token
                // Например: await _emailService.SendPasswordResetEmailAsync(user.Email, token);

                _logger.LogInformation("Password reset token generated for user: {UserId}", user.Id);

                return ApiResponse<bool>.SuccessResult(true, "Ако email адресът съществува, ще получите инструкции за възстановяване");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during forgot password for email: {Email}", request.Email);
                return ApiResponse<bool>.ErrorResult("Възникна грешка при заявката за възстановяване на паролата");
            }
        }

        /// <summary>
        /// Получава профил на потребител
        /// </summary>
        public async Task<ApiResponse<UserProfileDto>> GetUserProfileAsync(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return ApiResponse<UserProfileDto>.ErrorResult("Потребителят не е намерен");
                }

                var userProfile = await CreateUserProfileDtoAsync(user);
                return ApiResponse<UserProfileDto>.SuccessResult(userProfile);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user profile for user: {UserId}", userId);
                return ApiResponse<UserProfileDto>.ErrorResult("Възникна грешка при извличането на профила");
            }
        }

        /// <summary>
        /// Логин на потребител
        /// </summary>
        public async Task<ApiResponse<LoginResponse>> LoginAsync(LoginRequest request)
        {
            try
            {
                _logger.LogInformation("Attempting login for user: {Email}", request.Email);

                var user = await _userManager.FindByEmailAsync(request.Email);
                if (user == null)
                {
                    _logger.LogWarning("Login attempt for non-existent user: {Email}", request.Email);
                    return ApiResponse<LoginResponse>.ErrorResult("Невалиден email или парола");
                }

                if (!user.IsActive)
                {
                    _logger.LogWarning("Login attempt for inactive user: {UserId}", user.Id);
                    return ApiResponse<LoginResponse>.ErrorResult("Акаунтът е деактивиран");
                }

                var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);

                if (!result.Succeeded)
                {
                    if (result.IsLockedOut)
                    {
                        _logger.LogWarning("User {UserId} account is locked out", user.Id);
                        return ApiResponse<LoginResponse>.ErrorResult("Акаунтът е временно блокиран поради много неуспешни опити за влизане");
                    }

                    _logger.LogWarning("Failed login attempt for user: {Email}", request.Email);
                    return ApiResponse<LoginResponse>.ErrorResult("Невалиден email или парола");
                }

                // Генерираме токени
                var roles = await _userManager.GetRolesAsync(user);
                var accessToken = await _jwtService.GenerateAccessTokenAsync(user, roles.ToList());
                var refreshToken = _jwtService.GenerateRefreshToken();

                // Запазваме refresh token
                user.RefreshToken = refreshToken;
                user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7); // 7 дни
                await _userManager.UpdateAsync(user);

                var userProfile = await CreateUserProfileDtoAsync(user);

                var loginResponse = new LoginResponse
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(int.Parse(_configuration["JwtSettings:ExpirationMinutes"] ?? "60")),
                    User = userProfile
                };

                _logger.LogInformation("Successful login for user: {UserId}", user.Id);

                return ApiResponse<LoginResponse>.SuccessResult(loginResponse, "Успешно влизане");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for email: {Email}", request.Email);
                return ApiResponse<LoginResponse>.ErrorResult("Възникна грешка при влизането");
            }
        }

        /// <summary>
        /// Logout потребител
        /// </summary>
        public async Task<ApiResponse<bool>> LogoutAsync(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return ApiResponse<bool>.ErrorResult("Потребителят не е намерен");
                }

                // Инвалидираме refresh token
                user.RefreshToken = null;
                user.RefreshTokenExpiryTime = null;
                await _userManager.UpdateAsync(user);

                _logger.LogInformation("User {UserId} logged out successfully", userId);

                return ApiResponse<bool>.SuccessResult(true, "Успешно излизане");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout for user: {UserId}", userId);
                return ApiResponse<bool>.ErrorResult("Възникна грешка при излизането");
            }
        }

        /// <summary>
        /// Refresh token операция
        /// </summary>
        public async Task<ApiResponse<LoginResponse>> RefreshTokenAsync(RefreshTokenRequest request)
        {
            try
            {
                _logger.LogInformation("Attempting token refresh");

                var principal = _jwtService.GetPrincipalFromExpiredToken(request.AccessToken);
                var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);

                if (string.IsNullOrEmpty(userId))
                {
                    return ApiResponse<LoginResponse>.ErrorResult("Невалиден токен");
                }

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null || !_jwtService.ValidateRefreshToken(user, request.RefreshToken))
                {
                    return ApiResponse<LoginResponse>.ErrorResult("Невалиден refresh token");
                }

                // Генерираме нови токени
                var roles = await _userManager.GetRolesAsync(user);
                var newAccessToken = await _jwtService.GenerateAccessTokenAsync(user, roles.ToList());
                var newRefreshToken = _jwtService.GenerateRefreshToken();

                // Обновяваме refresh token
                user.RefreshToken = newRefreshToken;
                user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
                await _userManager.UpdateAsync(user);

                var userProfile = await CreateUserProfileDtoAsync(user);

                var loginResponse = new LoginResponse
                {
                    AccessToken = newAccessToken,
                    RefreshToken = newRefreshToken,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(int.Parse(_configuration["JwtSettings:ExpirationMinutes"] ?? "60")),
                    User = userProfile
                };

                _logger.LogInformation("Successfully refreshed token for user: {UserId}", user.Id);

                return ApiResponse<LoginResponse>.SuccessResult(loginResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during token refresh");
                return ApiResponse<LoginResponse>.ErrorResult("Възникна грешка при refresh на токена");
            }
        }

        /// <summary>
        /// Регистрира нов потребител
        /// </summary>
        public async Task<ApiResponse<UserProfileDto>> RegisterAsync(RegisterRequest request)
        {
            try
            {
                _logger.LogInformation("Attempting to register user with email: {Email}", request.Email);

                // Проверяваме дали потребителят вече съществува
                var existingUser = await _userManager.FindByEmailAsync(request.Email);
                if (existingUser != null)
                {
                    return ApiResponse<UserProfileDto>.ErrorResult("Потребител с този email адрес вече съществува");
                }

                // Създаваме новия потребител
                var user = new ApplicationUser
                {
                    UserName = request.Email,
                    Email = request.Email,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    PhoneNumber = request.PhoneNumber,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    EmailConfirmed = false // Може да се настрои според нуждите
                };

                var result = await _userManager.CreateAsync(user, request.Password);

                if (!result.Succeeded)
                {
                    var errors = result.Errors.Select(e => e.Description).ToList();
                    _logger.LogWarning("Failed to register user {Email}. Errors: {Errors}",
                        request.Email, string.Join(", ", errors));
                    return ApiResponse<UserProfileDto>.ErrorResult(errors);
                }

                // Добавяме роля "Customer" по подразбиране
                await EnsureRoleExistsAsync("Customer");
                var roleResult = await _userManager.AddToRoleAsync(user, "Customer");

                if (!roleResult.Succeeded)
                {
                    _logger.LogWarning("Failed to add role Customer to user {UserId}", user.Id);
                }

                _logger.LogInformation("Successfully registered user {UserId} with email {Email}",
                    user.Id, user.Email);

                // Генерираме email confirmation token (ако е нужно)
                // var emailToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                // TODO: Изпращане на confirmation email

                var userProfile = await CreateUserProfileDtoAsync(user);
                return ApiResponse<UserProfileDto>.SuccessResult(userProfile, "Регистрацията е успешна");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during user registration for email: {Email}", request.Email);
                return ApiResponse<UserProfileDto>.ErrorResult("Възникна грешка при регистрацията");
            }
        }

        /// <summary>
        /// Възстановява парола с token
        /// </summary>
        public async Task<ApiResponse<bool>> ResetPasswordAsync(ResetPasswordRequest request)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(request.Email);
                if (user == null)
                {
                    return ApiResponse<bool>.ErrorResult("Невалиден токен или email адрес");
                }

                var result = await _userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);
                if (!result.Succeeded)
                {
                    var errors = result.Errors.Select(e => e.Description).ToList();
                    return ApiResponse<bool>.ErrorResult(errors);
                }

                _logger.LogInformation("Password reset successfully for user: {UserId}", user.Id);

                return ApiResponse<bool>.SuccessResult(true, "Паролата е възстановена успешно");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during password reset for email: {Email}", request.Email);
                return ApiResponse<bool>.ErrorResult("Възникна грешка при възстановяването на паролата");
            }
        }

        /// <summary>
        /// Обновява профил на потребител
        /// </summary>
        public async Task<ApiResponse<UserProfileDto>> UpdateProfileAsync(string userId, UpdateProfileRequest request)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return ApiResponse<UserProfileDto>.ErrorResult("Потребителят не е намерен");
                }

                user.FirstName = request.FirstName;
                user.LastName = request.LastName;
                user.PhoneNumber = request.PhoneNumber;
                user.UpdatedAt = DateTime.UtcNow;

                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    var errors = result.Errors.Select(e => e.Description).ToList();
                    return ApiResponse<UserProfileDto>.ErrorResult(errors);
                }

                _logger.LogInformation("Updated profile for user: {UserId}", userId);

                var userProfile = await CreateUserProfileDtoAsync(user);
                return ApiResponse<UserProfileDto>.SuccessResult(userProfile, "Профилът е обновен успешно");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating profile for user: {UserId}", userId);
                return ApiResponse<UserProfileDto>.ErrorResult("Възникна грешка при обновяването на профила");
            }
        }

        // ========================================
        // Helper Methods
        // ========================================

        /// <summary>
        /// Създава UserProfileDto от ApplicationUser
        /// </summary>
        private async Task<UserProfileDto> CreateUserProfileDtoAsync(ApplicationUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);

            return new UserProfileDto
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                FirstName = user.FirstName,
                LastName = user.LastName,
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber,
                EmailConfirmed = user.EmailConfirmed,
                PhoneNumberConfirmed = user.PhoneNumberConfirmed,
                CreatedAt = user.CreatedAt,
                Roles = roles.ToList()
            };
        }

        /// <summary>
        /// Осигурява че ролята съществува
        /// </summary>
        private async Task EnsureRoleExistsAsync(string roleName)
        {
            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                await _roleManager.CreateAsync(new IdentityRole(roleName));
                _logger.LogInformation("Created role: {RoleName}", roleName);
            }
        }
    }
}
