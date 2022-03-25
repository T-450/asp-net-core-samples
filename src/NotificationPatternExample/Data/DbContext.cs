using Microsoft.EntityFrameworkCore;
using NotificationPatternExample.Business.Models;

namespace NotificationPatternExample.Data;

public class DbContext : Microsoft.EntityFrameworkCore.DbContext
{
    public DbContext(DbContextOptions<DbContext> options)
        : base(options)
    {
    }

    public DbSet<Product> Products { get; set; } = null!;
}