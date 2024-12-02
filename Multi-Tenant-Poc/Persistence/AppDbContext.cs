using Microsoft.EntityFrameworkCore;
using Multi_Tenant_Poc.Models;

namespace Multi_Tenant_Poc.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        AppContext.SetSwitch("Npgsql.DisableDateTimeInfinityConversions", true);
    }

    public DbSet<WeatherForecast> Forecasts => Set<WeatherForecast>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<WeatherForecast>()
            .HasData(
                new WeatherForecast
                {
                    Id = 1,
                    Date = new(2022, 1, 1),
                    Summary = "Chilly",
                    TemperatureC = -17
                },
                new WeatherForecast
                {
                    Id = 2,
                    Date = new(2022, 1, 2),
                    Summary = "Balmy",
                    TemperatureC = 38
                },
                new WeatherForecast
                {
                    Id = 3,
                    Date = new(2022, 1, 3),
                    Summary = "Sweltering",
                    TemperatureC = -7
                });
    }
}