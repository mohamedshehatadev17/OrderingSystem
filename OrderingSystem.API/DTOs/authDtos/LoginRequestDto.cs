using System.ComponentModel.DataAnnotations;

namespace OrderingSystem.API.DTOs.authDtos
{
    public class LoginRequestDto
    {
        [Required, EmailAddress]
        public required string Email { get; set; }

        [Required]
        public required string Password { get; set; }
    }
}
