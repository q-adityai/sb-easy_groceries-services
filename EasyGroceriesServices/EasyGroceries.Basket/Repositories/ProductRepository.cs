using System.Linq;
using System.Threading.Tasks;
using EasyGroceries.Basket.Model.Context;
using EasyGroceries.Basket.Model.Entities;
using EasyGroceries.Basket.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EasyGroceries.Basket.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly BasketContext _context;

    public ProductRepository(BasketContext context)
    {
        _context = context;
        _context.Database.EnsureCreated();
    }

    public async Task<Product> SaveProductAsync(Product product)
    {
        var trackedProduct = _context.Products.Update(product);
        await _context.SaveChangesAsync();

        return trackedProduct.Entity;
    }
    
    public async Task<Product> AddProductAsync(Product product)
    {
        var trackedProduct = await _context.Products.AddAsync(product);
        await _context.SaveChangesAsync();

        return trackedProduct.Entity;
    }

    public async Task<Product?> GetProductById(string productId)
    {
        return await _context.Products.Where(p => p.Id == productId).FirstOrDefaultAsync();
    }
}