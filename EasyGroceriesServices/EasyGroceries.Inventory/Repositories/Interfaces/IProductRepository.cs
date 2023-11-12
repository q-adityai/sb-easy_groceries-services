using System.Collections.Generic;
using System.Threading.Tasks;
using EasyGroceries.Inventory.Model.Entities;

namespace EasyGroceries.Inventory.Repositories.Interfaces;

public interface IProductRepository
{
    Task<List<Product>> GetAllApplicableProducts();
}