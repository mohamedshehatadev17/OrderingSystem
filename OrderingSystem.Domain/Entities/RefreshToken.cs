using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace OrderingSystem.Domain.Entities
{
    public class RefreshToken
    {
        public int Id { get; set; }

        public string TokenHash { get; set; } = null!;

        public DateTime ExpiresAtUtc { get; set; }

        public DateTime? RevokedAtUtc { get; set; }
        [ForeignKey("Customer")]
        public int CustomerId { get; set; }

        public Customer Customer { get; set; } = null!;

        public bool IsExpired => DateTime.UtcNow >= ExpiresAtUtc;

        public bool IsRevoked => RevokedAtUtc.HasValue;

        public bool IsActive => !IsExpired && !IsRevoked;
    }
}
