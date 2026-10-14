using KampConnect.Shared.Contracts.Authorization;
using KampConnect.Shared.ServiceDefaults.Authentication;
using KampConnect.Shared.ServiceDefaults.Messaging;
using KampConnect.Tenant.Data.Context.Implentation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<KampConnectTenantDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("kampconnect_tenant"));
});

// Add controllers
builder.Services.AddControllers();

// Add Data
builder.Services.AddTenantData();

// Add Business 
builder.Services.AddTenantBusiness();

// Add authentication
builder.AddKampConnectJwtAuthentication();

builder.Services.AddKampConnectAuthorization();

builder.AddKampConnectMessaging(typeof(Program).Assembly);

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
