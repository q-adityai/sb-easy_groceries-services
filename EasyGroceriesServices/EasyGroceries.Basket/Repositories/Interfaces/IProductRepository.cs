using System.Threading.Tasks;
using EasyGroceries.Basket.Model.Entities;

namespace EasyGroceries.Basket.Repositories.Interfaces;

public interface IProductRepository
{
    Task<Product> CreateProductAsync(Product product);
}