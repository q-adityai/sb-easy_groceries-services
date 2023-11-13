using EasyGroceries.Basket.Model.Entities;
using EasyGroceries.Common.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace EasyGroceries.Basket.Model.Context;

public class BasketContext : DbContext
{
    private readonly CosmosDbOptions _options;

    public BasketContext(IOptions<CosmosDbOptions> options)
    {
        _options = options.Value;
    }

    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Product> Products { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseCosmos(_options.Uri, _options.Key, _options.DatabaseName);
        base.OnConfiguring(optionsBuilder);
    }
}