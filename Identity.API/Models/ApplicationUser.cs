using Microsoft.AspNetCore.Identity;

namespace Identity.API.Models
{
    /// <summary>
    /// Разширен потребителски модел за Identity
    /// Наследява IdentityUser за пълна функционалност
    /// </summary>
    public class ApplicationUser : IdentityUser
    {
        /// <summary>
        /// Пълно име на потребителя
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// Фамилия на потребителя
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// Дата на създаване на профила
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Дата на последна актуализация
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Дали профилът е активен
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Refresh token за JWT
        /// </summary>
        public string RefreshToken { get; set; }

        /// <summary>
        /// Дата на изтичане на refresh token
        /// </summary>
        public DateTime? RefreshTokenExpiryTime { get; set; }

        /// <summary>
        /// Пълно име (computed property)
        /// </summary>
        public string FullName => $"{FirstName} {LastName}".Trim();
    }
}