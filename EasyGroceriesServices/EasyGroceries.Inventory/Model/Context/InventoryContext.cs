using EasyGroceries.Common.Configuration;
using EasyGroceries.Inventory.Model.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace EasyGroceries.Inventory.Model.Context;

public class InventoryContext : DbContext
{
    private readonly CosmosDbOptions _options;
    public InventoryContext(IOptions<CosmosDbOptions> options)
    {
        _options = options.Value;
    }

    public DbSet<Category> Categories { get; set; }
    public DbSet<Product> Products { get; set; }
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseCosmos(_options.Uri, _options.Key, _options.DatabaseName);
        base.OnConfiguring(optionsBuilder);
    }
}