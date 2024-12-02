// See https://aka.ms/new-console-template for more information

using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Migrator_Test;
using Multi_Tenant_Poc.Persistence;
using Multi_Tenant_Poc.TenantConfig;

var builder = Host.CreateApplicationBuilder(args);
var customConfigPath = Path.Combine(Directory.GetCurrentDirectory(), "../Multi-Tenant-Poc");
builder.Configuration.SetBasePath(customConfigPath)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddSingleton<IConfiguration>(builder.Configuration);
builder.Services.AddScoped<ITenantResolver, TenantResolver>();
builder.Services.AddScoped<IDbContextFactory<AppDbContext>, AppDbContextFactory>();
builder.Services.AddHostedService<MigratorService>();


var host = builder.Build();
host.Run();


