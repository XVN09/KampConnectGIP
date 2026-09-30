using KampConnect.Tenant.Data.Context.Implentation;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<KampConnectTenantDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("kampconnect_tenant"));
});

builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
