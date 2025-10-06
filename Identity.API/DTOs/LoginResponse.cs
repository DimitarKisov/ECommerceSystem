namespace Identity.API.DTOs
{
    /// <summary>
    /// DTO за отговор при успешен логин
    /// </summary>
    public class LoginResponse
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public DateTime ExpiresAt { get; set; }
        public UserProfileDto User { get; set; } = new();
    }
}
