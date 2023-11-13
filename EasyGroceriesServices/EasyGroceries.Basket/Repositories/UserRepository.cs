using System.Threading.Tasks;
using EasyGroceries.Basket.Model.Context;
using EasyGroceries.Basket.Model.Entities;
using EasyGroceries.Basket.Repositories.Interfaces;

namespace EasyGroceries.Basket.Repositories;

public class UserRepository : IUserRepository
{
    private readonly BasketContext _context;

    public UserRepository(BasketContext context)
    {
        _context = context;
        _context.Database.EnsureCreated();
    }

    public async Task<User> CreateUserAsync(User user)
    {
        var trackedUser = await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        return trackedUser.Entity;
    }
}