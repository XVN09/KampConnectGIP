using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using KampConnect.Tenant.Data.Entity.Core;

namespace KampConnect.Tenant.Data.Context.Implentation;

public class KampConnectTenantDbContext(
    DbContextOptions<KampConnectTenantDbContext> options) : DbContext(options)
{
    public DbSet<KampConnect.Tenant.Data.Entity.Core.Tenant> Tenants { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        // Configure the Tenant entity
        modelBuilder.Entity<KampConnect.Tenant.Data.Entity.Core.Tenant>(entity =>
        {
            entity.HasKey(e => e.tenant_id);
            entity.Property(e => e.tenant_name).IsRequired();
            entity.Property(e => e.tenant_slug).IsRequired();
            entity.Property(e => e.tenant_status).IsRequired();
            entity.Property(e => e.created_at).IsRequired();
        });
    }
}
