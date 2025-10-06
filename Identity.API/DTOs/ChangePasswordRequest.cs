using System.ComponentModel.DataAnnotations;

namespace Identity.API.DTOs
{
    /// <summary>
    /// DTO за смяна на парола
    /// </summary>
    public class ChangePasswordRequest
    {
        [Required(ErrorMessage = "Текущата парола е задължителна")]
        [DataType(DataType.Password)]
        public string CurrentPassword { get; set; }

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
