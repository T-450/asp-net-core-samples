using Microsoft.EntityFrameworkCore;

namespace Product.Microservice.Data;

public interface IApplicationDbContext
{
    DbSet<Entities.Product> Products { get; set; }
    Task<int> SaveChanges();
}
