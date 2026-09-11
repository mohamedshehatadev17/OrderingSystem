
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
namespace OrderingSystem.Domain.Entities
{
    [Table("AspNetUsers")]
    public class Customer : IdentityUser<int>
    {
        public string Name { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? BannedUntil { get; set; }

        public List<Order> Orders { get; set; } = new();
        public List<RefreshToken> RefreshTokens { get; set; } = new();
        
    }
}
