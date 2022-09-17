using Microsoft.EntityFrameworkCore;

namespace Product.Microservice.Data;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Entities.Product> Products { get; set; }

    public async Task<int> SaveChanges()
    {
        return await base.SaveChangesAsync();
    }
}
