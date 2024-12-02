using Microsoft.EntityFrameworkCore;
using Multi_Tenant_Poc.TenantConfig;

namespace Multi_Tenant_Poc.Persistence;
public class AppDbContextFactory(ITenantResolver tenantResolver) : IDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext()
    {
        var tenantInfo = tenantResolver.GetCurrentTenant();

        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

        // Configure the DbContext based on the provider
        switch (tenantInfo.Provider.ToLower())
        {
            case "postgresql":
                optionsBuilder.UseNpgsql(tenantInfo.ConnectionString);
                break;
            case "memory":
                optionsBuilder.UseInMemoryDatabase(tenantInfo.ConnectionString);
                break;
            default:
                throw new NotSupportedException($"The provider {tenantInfo.Provider} is not supported.");
        }

        return new AppDbContext(optionsBuilder.Options);;
    }
}