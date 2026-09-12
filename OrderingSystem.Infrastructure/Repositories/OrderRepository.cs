using OrderingSystem.Domain.Entities;
using OrderingSystem.Domain.Interfaces;
using OrderingSystem.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace OrderingSystem.Infrastructure.Repositories
{
    public class OrderRepository : GenericRepository<Order>, IOrderRepository
    {
        public OrderRepository(ApplicationDbContext context): base(context)
        {
            
        }
    }
}
