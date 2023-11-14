using Azure.Messaging.ServiceBus.Administration;
using EasyGroceries.Basket.Model.Entities;
using EasyGroceries.Common.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace EasyGroceries.Basket.Model.Context;

public class BasketContext : DbContext
{

    public BasketContext(DbContextOptions<BasketContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Product> Products { get; set; } = null!;
    public DbSet<Entities.Basket> Baskets { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
            .OwnsOne(u => u.DefaultBillingAddress)
            .WithOwner();
        
        modelBuilder.Entity<User>()
            .OwnsOne(u => u.DefaultDeliveryAddress)
            .WithOwner();
        
        modelBuilder.Entity<Product>()
            .OwnsOne(u => u.Price)
            .WithOwner();
        
        modelBuilder.Entity<Product>()
            .OwnsOne(u => u.DiscountedPrice)
            .WithOwner();

        modelBuilder.Entity<Entities.Basket>()
            .OwnsMany(b => b.Products)
            .WithOwner();
        
        base.OnModelCreating(modelBuilder);
    }
}