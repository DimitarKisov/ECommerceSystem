using System.ComponentModel.DataAnnotations;

namespace Identity.API.DTOs
{
    /// <summary>
    /// DTO за регистрация на нов потребител
    /// </summary>
    public class RegisterRequest
    {
        [Required(ErrorMessage = "Email адресът е задължителен")]
        [EmailAddress(ErrorMessage = "Невалиден email адрес")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Паролата е задължителна")]
        [StringLength(100, ErrorMessage = "Паролата трябва да е между {2} и {1} символа", MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "Потвърждението на паролата е задължително")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Паролите не съвпадат")]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "Името е задължително")]
        [StringLength(50, ErrorMessage = "Името не може да бъде по-дълго от {1} символа")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Фамилията е задължителна")]
        [StringLength(50, ErrorMessage = "Фамилията не може да бъде по-дълга от {1} символа")]
        public string LastName { get; set; }

        [Phone(ErrorMessage = "Невалиден телефонен номер")]
        public string PhoneNumber { get; set; }
    }
}
