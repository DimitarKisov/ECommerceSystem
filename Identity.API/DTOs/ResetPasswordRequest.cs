using System.ComponentModel.DataAnnotations;

namespace Identity.API.DTOs
{
    /// <summary>
    /// DTO за възстановяване на парола
    /// </summary>
    public class ResetPasswordRequest
    {
        [Required(ErrorMessage = "Email адресът е задължителен")]
        [EmailAddress(ErrorMessage = "Невалиден email адрес")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Токенът е задължителен")]
        public string Token { get; set; }

        [Required(ErrorMessage = "Новата парола е задължителна")]
        [StringLength(100, ErrorMessage = "Паролата трябва да е между {2} и {1} символа", MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "Потвърждението на новата парола е задължително")]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Паролите не съвпадат")]
        public string ConfirmNewPassword { get; set; }
    }
}
