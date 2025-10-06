using System.ComponentModel.DataAnnotations;

namespace Identity.API.DTOs
{
    /// <summary>
    /// DTO за refresh token заявка
    /// </summary>
    public class RefreshTokenRequest
    {
        [Required(ErrorMessage = "Access token е задължителен")]
        public string AccessToken { get; set; }

        [Required(ErrorMessage = "Refresh token е задължителен")]
        public string RefreshToken { get; set; }
    }
}
