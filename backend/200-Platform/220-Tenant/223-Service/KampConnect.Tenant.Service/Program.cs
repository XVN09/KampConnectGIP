using KampConnect.Shared.Contracts.Authorization;
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
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = builder.Configuration["Authentication:Authority"];
        options.Audience = builder.Configuration["Authentication:Audience"];
        options.MapInboundClaims = false;
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(AuthorizationConstants.RequiresTenantPolicy, policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireClaim(KampConnectClaimTypes.TenantId);
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
