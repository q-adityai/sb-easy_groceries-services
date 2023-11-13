using System.Threading.Tasks;
using EasyGroceries.Basket.Model.Entities;

namespace EasyGroceries.Basket.Repositories.Interfaces;

public interface IUserRepository
{
    Task<User> CreateUserAsync(User user);
}