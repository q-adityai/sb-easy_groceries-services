using System.Threading.Tasks;
using EasyGroceries.Basket.Model.Context;
using EasyGroceries.Basket.Model.Entities;
using EasyGroceries.Basket.Repositories.Interfaces;

namespace EasyGroceries.Basket.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly BasketContext _context;

    public ProductRepository(BasketContext context)
    {
        _context = context;
        _context.Database.EnsureCreated();
    }

    public async Task<Product> CreateProductAsync(Product product)
    {
        var trackedProduct = await _context.Products.AddAsync(product);
        await _context.SaveChangesAsync();

        return trackedProduct.Entity;
    }
}