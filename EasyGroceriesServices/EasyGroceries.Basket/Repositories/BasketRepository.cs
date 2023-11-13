using System.Linq;
using System.Threading.Tasks;
using EasyGroceries.Basket.Model.Context;
using EasyGroceries.Basket.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EasyGroceries.Basket.Repositories;

public class BasketRepository : IBasketRepository
{
    private readonly BasketContext _context;

    public BasketRepository(BasketContext context)
    {
        _context = context;
        _context.Database.EnsureDeleted();
        _context.Database.EnsureCreated();
    }
    
    public async Task<Model.Entities.Basket?> GetBasketAsync(string basketId)
    {
        return await _context.Baskets.Where(b => b.Id == basketId).FirstOrDefaultAsync();
    }
    
    public async Task<Model.Entities.Basket> CreateBasketAsync(Model.Entities.Basket basket)
    {
        var trackedBasket = _context.Baskets.Add(basket);
        await _context.SaveChangesAsync();

        return trackedBasket.Entity;
    }

    public async Task<Model.Entities.Basket> SaveBasketAsync(Model.Entities.Basket basket)
    {
        var trackedBasket = _context.Baskets.Update(basket);
        await _context.SaveChangesAsync();

        return trackedBasket.Entity;
    }
}