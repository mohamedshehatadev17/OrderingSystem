using System;
using System.Collections.Generic;
using System.Text;

namespace OrderingSystem.Infrastructure.JwtToken
{
    public class JwtOptions
    {

            public const string SectionName = "Jwt";
            public string Issuer { get; set; } = string.Empty;
            public string Audience { get; set; } = string.Empty;
            public string Key { get; set; } = string.Empty; // load from env/user-secrets/Key Vault, not source control
            public int AccessTokenMinutes { get; set; } = 30;
            public int RefreshTokenDays { get; set; } = 7;
    }
}
