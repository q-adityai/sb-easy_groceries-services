using EasyGroceries.Inventory.Model.Entities;
using Microsoft.EntityFrameworkCore;

namespace EasyGroceries.Inventory.Model.Context;

public class InventoryContext : DbContext
{
    public InventoryContext(DbContextOptions<InventoryContext> options) : base(options)
    {
    }

    public DbSet<Product> Products { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>()
            .OwnsOne(p => p.Price)
            .WithOwner();
        
        modelBuilder.Entity<Product>()
            .OwnsOne(p => p.Sku)
            .WithOwner();

        base.OnModelCreating(modelBuilder);
    }
}