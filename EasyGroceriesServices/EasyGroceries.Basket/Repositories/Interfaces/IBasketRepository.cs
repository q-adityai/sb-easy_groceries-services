using System.Threading.Tasks;

namespace EasyGroceries.Basket.Repositories.Interfaces;

public interface IBasketRepository
{
    Task<Model.Entities.Basket?> GetBasketAsync(string basketId);

    Task<Model.Entities.Basket> SaveBasketAsync(Model.Entities.Basket basket);
}