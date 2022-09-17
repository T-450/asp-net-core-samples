using Microsoft.EntityFrameworkCore;

namespace Customer.Microservice.Data;

public interface IApplicationDbContext
{
    DbSet<Entities.Customer> Customers { get; set; }
    Task<int> SaveChanges();
}
