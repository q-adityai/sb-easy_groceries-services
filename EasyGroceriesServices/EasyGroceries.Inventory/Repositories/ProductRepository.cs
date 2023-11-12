using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EasyGroceries.Inventory.Model.Context;
using EasyGroceries.Inventory.Model.Entities;
using EasyGroceries.Inventory.Repositories.Interfaces;

namespace EasyGroceries.Inventory.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly InventoryContext _context;

    public ProductRepository(InventoryContext context)
    {
        _context = context;
        _context.Database.EnsureDeleted();
        _context.Database.EnsureCreated();
    }

    public async Task<List<Product>> GetAllApplicableProducts()
    {
        var products = _context.Products.ToList();
        var currentDateTime = DateTimeOffset.UtcNow;
        
        return await Task.FromResult(products.FindAll(p => currentDateTime >= p.ValidFrom && currentDateTime <= p.ValidTo));
    }
}