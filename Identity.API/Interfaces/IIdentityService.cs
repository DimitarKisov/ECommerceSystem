using Identity.API.DTOs;

namespace Identity.API.Interfaces
{
    /// <summary>
    /// Интерфейс за Identity операции
    /// </summary>
    public interface IIdentityService
    {
        Task<ApiResponse<UserProfileDto>> RegisterAsync(RegisterRequest request);
        Task<ApiResponse<LoginResponse>> LoginAsync(LoginRequest request);
        Task<ApiResponse<LoginResponse>> RefreshTokenAsync(RefreshTokenRequest request);
        Task<ApiResponse<bool>> LogoutAsync(string userId);
        Task<ApiResponse<UserProfileDto>> GetUserProfileAsync(string userId);
        Task<ApiResponse<UserProfileDto>> UpdateProfileAsync(string userId, UpdateProfileRequest request);
        Task<ApiResponse<bool>> ChangePasswordAsync(string userId, ChangePasswordRequest request);
        Task<ApiResponse<bool>> ForgotPasswordAsync(ForgotPasswordRequest request);
        Task<ApiResponse<bool>> ResetPasswordAsync(ResetPasswordRequest request);
        Task<ApiResponse<bool>> ConfirmEmailAsync(string userId, string token);
    }
}
