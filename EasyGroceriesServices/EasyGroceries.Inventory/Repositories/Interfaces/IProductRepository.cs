using System.Collections.Generic;
using System.Threading.Tasks;
using EasyGroceries.Inventory.Model.Entities;

namespace EasyGroceries.Inventory.Repositories.Interfaces;

public interface IProductRepository
{
    Task<List<Product>> GetAllApplicableProductsAsync();
    Task<List<Product>> GetProductsAsync();

    Task<Product?> GetProductByNameAsync(string name);

    Task<Product> CreateProductAsync(Product product);
}