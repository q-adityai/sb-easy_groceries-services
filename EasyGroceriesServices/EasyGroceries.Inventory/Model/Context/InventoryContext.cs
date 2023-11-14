using EasyGroceries.Common.Configuration;
using EasyGroceries.Common.Entities;
using EasyGroceries.Inventory.Model.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace EasyGroceries.Inventory.Model.Context;

public class InventoryContext : DbContext
{
    public InventoryContext(DbContextOptions<InventoryContext> options) : base(options)
    {
    }

    public DbSet<Product> Products { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>(p =>
        {
            p.Ignore(m => m.Price);
            p.Ignore(s => s.Sku);
        });

        //modelBuilder.Entity<Product>().OwnsOne<Money>(p => p.Price);
        base.OnModelCreating(modelBuilder);
    }
}