using Customer.Microservice.Entities;
using Microsoft.EntityFrameworkCore;

namespace Customer.Microservice.Data;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<EntityBase>? EntityBase { get; set; }
    public DbSet<Entities.Customer> Customers { get; set; } = null!;

    public new async Task<int> SaveChanges()
    {
        return await base.SaveChangesAsync();
    }
}
