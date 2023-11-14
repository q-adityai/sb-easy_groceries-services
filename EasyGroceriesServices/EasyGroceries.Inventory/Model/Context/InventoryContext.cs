using EasyGroceries.Common.Configuration;
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
}