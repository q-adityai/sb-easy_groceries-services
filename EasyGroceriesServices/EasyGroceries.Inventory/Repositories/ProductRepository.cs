using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EasyGroceries.Inventory.Model.Context;
using EasyGroceries.Inventory.Model.Entities;
using EasyGroceries.Inventory.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

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

    public async Task<List<Product>> GetAllApplicableProductsAsync()
    {
        var currentDateTime = DateTimeOffset.UtcNow;
        var products = _context.Products.Where(p => currentDateTime >= p.ValidFrom && currentDateTime <= p.ValidTo).ToList();
        
        return await Task.FromResult(products);
    }
    
    public async Task<List<Product>> GetProductsAsync()
    {
        return await Task.FromResult(_context.Products.ToList());
    }

    public async Task<Product?> GetProductByNameAsync(string name)
    {
        return await _context.Products.Where(p => p.Name == name)
            .FirstOrDefaultAsync();
    }

    public async Task<Product> CreateProductAsync(Product product)
    {
        var trackedProduct = await _context.Products.AddAsync(product);
        await _context.SaveChangesAsync();

        return trackedProduct.Entity;
    }
}