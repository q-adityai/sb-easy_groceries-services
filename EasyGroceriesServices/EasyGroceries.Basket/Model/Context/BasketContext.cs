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
}