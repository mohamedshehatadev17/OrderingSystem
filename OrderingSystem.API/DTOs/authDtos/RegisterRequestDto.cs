using System.ComponentModel.DataAnnotations;

namespace OrderingSystem.API.DTOs.authDtos
{
    public class RegisterRequestDto
    {
        [Required, StringLength(100, MinimumLength = 2)]
        public required string Name { get; set; }

        [Required, EmailAddress, StringLength(150)]
        public required string Email { get; set; }

        [Required, StringLength(100, MinimumLength = 8)]
        public required string Password { get; set; }
        [Phone, StringLength(20)]
        public string? PhoneNumber { get; set; } = null;
    }
}
