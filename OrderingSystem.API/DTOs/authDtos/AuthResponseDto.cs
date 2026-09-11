namespace OrderingSystem.API.DTOs.authDtos
{
    public class AuthResponseDto
    {
        public int CustomerId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string AccessToken { get; set; } = string.Empty;
        public DateTime ExpiresAtUtc { get; set; }
        public string RefreshToken { get; set; } = string.Empty;
    }
}
