using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using OrderingSystem.Domain.Entities;
using OrderingSystem.Domain.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace OrderingSystem.Infrastructure.JwtToken
{

    public class TokenService(IOptions<JwtOptions> options) : ITokenService
    {
            private readonly JwtOptions _options = options.Value;

        public (string Token, DateTime ExpiresAtUtc) GenerateAccessToken(Customer customer, IEnumerable<string> roles)
        {
            var claims = new List<Claim>
    {
        new(JwtRegisteredClaimNames.Sub, customer.Id.ToString()),
        new(JwtRegisteredClaimNames.Email, customer.Email!),
        new(ClaimTypes.Name, customer.Name),
        new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
    };

            // Add each role as a standard ClaimTypes.Role claim
            if (roles != null)
            {
                foreach (var role in roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Key));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.UtcNow.AddMinutes(_options.AccessTokenMinutes);

            var token = new JwtSecurityToken(
                issuer: _options.Issuer,
                audience: _options.Audience,
                claims: claims,
                expires: expires,
                signingCredentials: creds);

            return (new JwtSecurityTokenHandler().WriteToken(token), expires);
        }

        public string GenerateRefreshToken() => Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        public string GenerateRefreshTokenHash(string token)
        {
            var bytes = SHA256.HashData(
                Encoding.UTF8.GetBytes(token));

            return Convert.ToBase64String(bytes);
        }
    }
}
