using System.Threading.Tasks;
using EasyGroceries.Basket.Repositories.Interfaces;

namespace EasyGroceries.Basket.Repositories;

public class BasketRepository : IBasketRepository
{
    public async Task<Model.Entities.Basket?> GetBasketAsync(string basketId)
    {
        throw new System.NotImplementedException();
    }

    public async Task<Model.Entities.Basket> SaveBasketAsync(Model.Entities.Basket basket)
    {
        throw new System.NotImplementedException();
    }
}