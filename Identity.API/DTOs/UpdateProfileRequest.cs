using System.ComponentModel.DataAnnotations;

namespace Identity.API.DTOs
{
    /// <summary>
    /// DTO за обновяване на профил
    /// </summary>
    public class UpdateProfileRequest
    {
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
