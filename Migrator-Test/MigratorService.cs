using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Multi_Tenant_Poc.Persistence;
using Multi_Tenant_Poc.TenantConfig;

namespace Migrator_Test;

public class MigratorService(
    IServiceProvider serviceProvider,
    IHostApplicationLifetime hostApplicationLifetime,
    IConfiguration configuration): BackgroundService
{
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            var args = Environment.GetCommandLineArgs();
        
            if (args.Length < 2)
            {
                Console.WriteLine("Please provide a valid command: 'migrate' or 'rollback'.");
                return;
            }

            var command = args[1].ToLower();
            switch (command)
            {
                case "migrate":
                    await ApplyMigrationsAsync();
                    break;
                case "rollback":
                    var migrationId = args.Length > 2 ? args[2] : null;
                    await RollbackMigrationsAsync(migrationId);
                    break;
                default:
                    Console.WriteLine("Invalid command. Use 'migrate' or 'rollback'.");
                    break;
            }   
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
        hostApplicationLifetime.StopApplication();
    }
    
     private async Task ApplyMigrationsAsync()
    {
        using (var scope = serviceProvider.CreateScope())
        {
            var dbContextFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>();

            // Retrieve tenant info and apply migration per tenant
            var tenantResolver = scope.ServiceProvider.GetRequiredService<ITenantResolver>();
            var mainDbContext = dbContextFactory.CreateDbContext();
            var mainMigrator = mainDbContext.Database.GetService<IMigrator>();
            Console.WriteLine($"Applying migrations for main tenant ...");
            await mainMigrator.MigrateAsync();
            Console.WriteLine($"Migrations applied successfully for main tenant");
            //Apply for Default DB
            var tenants =  GetTenantIds();

            foreach (var tenantId in tenants)
            {
                tenantResolver.SetCurrentTenant(tenantId);
                var dbContext = await dbContextFactory.CreateDbContextAsync();
                
                try
                {
                    Console.WriteLine($"Applying migrations for tenant {tenantId}...");
                    var migrator = dbContext.Database.GetService<IMigrator>();
                    await migrator.MigrateAsync();
                    Console.WriteLine($"Migrations applied successfully for tenant {tenantId}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error applying migrations for tenant {tenantId}: {ex.Message}");
                }
            }
        }
    }

    private async Task RollbackMigrationsAsync(string? migrationId = null)
    {
        using (var scope = serviceProvider.CreateScope())
        {
            var dbContextFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>();
            // Retrieve tenant info and rollback migration per tenant
            var tenantResolver = scope.ServiceProvider.GetRequiredService<ITenantResolver>();
            var mainDbContext = dbContextFactory.CreateDbContext();
            var mainMigrator = mainDbContext.Database.GetService<IMigrator>();
            if (string.IsNullOrEmpty(migrationId))
            {
                var latestMigrations = (await mainDbContext.Database.GetAppliedMigrationsAsync()).Reverse().Skip(1).FirstOrDefault();
                Console.WriteLine($"Rolling back to migration {latestMigrations} for main Tenant...");
                if (string.IsNullOrEmpty(latestMigrations))
                    Console.WriteLine($"No latest applied migrations for main Tenant...");
                await mainMigrator.MigrateAsync(latestMigrations); // Rolls back all migrations
                Console.WriteLine($"Migrations rolled back successfully for main Tenant");
            }
            else
            {
                // Rollback to a specific migration
                Console.WriteLine($"Rolling back to migration {migrationId} for main Tenant...");
                await mainMigrator.MigrateAsync(migrationId);
                Console.WriteLine($"Rolled back to migration {migrationId} for tenant main Tenant");
            }
            var tenants =  GetTenantIds();
            foreach (var tenantId in tenants)
            {
                tenantResolver.SetCurrentTenant(tenantId); // Set the current tenant for the resolver
                var dbContext = dbContextFactory.CreateDbContext();
                var migrator = dbContext.Database.GetService<IMigrator>();
                
                try
                {
                    if (string.IsNullOrEmpty(migrationId))
                    {
                        // Rollback to the previous migration
                        var latestAppliedMigrations = (await dbContext.Database.GetAppliedMigrationsAsync()).Reverse().Skip(1).FirstOrDefault();
                        Console.WriteLine($"Rolling back to migration {latestAppliedMigrations} for tenant {tenantId}...");
                        if (string.IsNullOrEmpty(latestAppliedMigrations))
                        {
                            Console.WriteLine($"No latest applied migrations for tenant {tenantId}...");
                            continue;
                        }
                        await migrator.MigrateAsync(latestAppliedMigrations); // Rolls back all migrations
                        Console.WriteLine($"Migrations rolled back successfully for tenant {tenantId}");
                    }
                    else
                    {
                        // Rollback to a specific migration
                        Console.WriteLine($"Rolling back to migration {migrationId} for tenant {tenantId}...");
                        await migrator.MigrateAsync(migrationId);
                        Console.WriteLine($"Rolled back to migration {migrationId} for tenant {tenantId}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error rolling back migrations for tenant {tenantId}: {ex.Message}");
                }
            }
        }
    }
    private string[] GetTenantIds()
    {
        var tenantsSection = configuration.GetSection("Tenants");
        var tenantNames = tenantsSection.GetChildren().Select(child => child.Key).ToArray();
        return tenantNames;
    }
}