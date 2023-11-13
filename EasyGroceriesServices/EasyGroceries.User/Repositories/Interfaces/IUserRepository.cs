using System.Collections.Generic;
using System.Threading.Tasks;

namespace EasyGroceries.User.Repositories.Interfaces;

public interface IUserRepository
{
    Task<List<Model.Entities.User>> GetUsersAsync(bool includeDeleted = false);
    Task<Model.Entities.User> CreateUserAsync(Model.Entities.User user);
    Task<Model.Entities.User?> GetUserAsync(string userId);
    Task<Model.Entities.User?> GetUserByEmailAsync(string email);
    Task<Model.Entities.User> SaveUserAsync(Model.Entities.User user);
}