using System.ComponentModel.DataAnnotations;

namespace Identity.API.DTOs
{
    /// <summary>
    /// DTO за забравена парола
    /// </summary>
    public class ForgotPasswordRequest
    {
        [Required(ErrorMessage = "Email адресът е задължителен")]
        [EmailAddress(ErrorMessage = "Невалиден email адрес")]
        public string Email { get; set; }
    }
}
