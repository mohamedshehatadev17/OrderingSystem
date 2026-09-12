using OrderingSystem.Domain.Entities;
using OrderingSystem.Domain.Interfaces;
using OrderingSystem.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace OrderingSystem.Infrastructure.Repositories
{
    public class CustomerRepository :GenericRepository<Customer>, ICustomerRepository  
    {
        public CustomerRepository(ApplicationDbContext context): base(context)
        {
            
        }
    }
}
