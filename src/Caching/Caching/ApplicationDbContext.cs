using Microsoft.EntityFrameworkCore;

namespace Caching
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<WeatherForecast> WeatherForecasts { get; set; }
        private readonly ILogger<ApplicationDbContext> _logger;
        public string DbPath { get; }


        public ApplicationDbContext(ILogger<ApplicationDbContext> logger)
        {
            _logger = logger;
            var folder = Environment.SpecialFolder.LocalApplicationData;
            var path = Environment.GetFolderPath(folder);
            DbPath = Path.Join(path, "database.db");
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options
                .UseSqlite($"Data Source={DbPath}")
                .LogTo(message => _logger.LogInformation(message));

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<WeatherForecast>().ToTable("WeatherForecast");
        }
    }
}
