namespace Multi_Tenant_Poc.TenantConfig;

public interface ITenantResolver
{
    Tenant GetCurrentTenant();
    void SetCurrentTenant(string tenantId);
}

public class TenantResolver : ITenantResolver
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IConfiguration _configuration;
    private string? _currentTenant;

    public TenantResolver(IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
    {
        _httpContextAccessor = httpContextAccessor;
        _configuration = configuration;
    }
    public void SetCurrentTenant(string tenantId)
    {
        _currentTenant = tenantId;
    }
    public Tenant GetCurrentTenant()
    {
        // Get tenant ID from header
        var tenantId = _currentTenant ?? _httpContextAccessor.HttpContext?.Request.Headers["X-Tenant-ID"].FirstOrDefault();

        if (!string.IsNullOrEmpty(tenantId))
        {
            // Retrieve tenant-specific settings from configuration
            var tenantSection = _configuration.GetSection($"Tenants:{tenantId}");
            if (tenantSection.Exists())
            {
                return new Tenant
                {
                    ConnectionString = tenantSection["ConnectionString"]!,
                    Provider = tenantSection["Provider"] ?? "PostgreSQL"
                };
            }
        }

        // Fall back to default connection string
        var defaultConnectionString = _configuration.GetConnectionString("Default");
        return new Tenant
        {
            ConnectionString = defaultConnectionString!,
            Provider = "PostgreSQL"
        };
    }
}

