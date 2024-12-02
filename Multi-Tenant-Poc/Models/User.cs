namespace Multi_Tenant_Poc.Models;

public class User
{
    public int Id { get; set; }
    public string Name { get; set; }
}

public class UserWithTenant : User
{
    public int TenantId { get; set; }
}

