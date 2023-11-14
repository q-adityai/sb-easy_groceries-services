using System.Threading.Tasks;
using EasyGroceries.Basket.Model.Entities;

namespace EasyGroceries.Basket.Repositories.Interfaces;

public interface IProductRepository
{
    Task<Product> AddProductAsync(Product product);
    Task<Product> SaveProductAsync(Product product);
    Task<Product?> GetProductById(string productId);
}