using Microsoft.EntityFrameworkCore;
using OrderingSystem.Domain.Entities;
using OrderingSystem.Domain.Interfaces;
using OrderingSystem.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace OrderingSystem.Infrastructure.Repositories
{
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly ApplicationDbContext _context;

        public RefreshTokenRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public Task AddAsync(RefreshToken refreshToken)
        {
            _context.RefreshTokens.Add(refreshToken);
            return _context.SaveChangesAsync();
        }
        public Task<RefreshToken?> GetByTokenHashAsync(string tokenHash)
        {
            return _context.RefreshTokens
                .Include(rt => rt.Customer)
                .FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash);
        }
    }
}
