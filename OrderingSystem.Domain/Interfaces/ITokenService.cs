using OrderingSystem.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace OrderingSystem.Domain.Interfaces
{
    public interface ITokenService
    {
        (string Token, DateTime ExpiresAtUtc) GenerateAccessToken(Customer customer, IEnumerable<string> roles);
        string GenerateRefreshToken();
    }
}
