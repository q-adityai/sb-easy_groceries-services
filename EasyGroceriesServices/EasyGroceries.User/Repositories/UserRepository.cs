using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EasyGroceries.User.Model.Context;
using EasyGroceries.User.Repositories.Interfaces;

namespace EasyGroceries.User.Repositories;

public class UserRepository : IUserRepository
{
    private readonly UserContext _context;

    public UserRepository(UserContext context)
    {
        _context = context;
        _context.Database.EnsureCreated();
    }

    public async Task<List<Model.Entities.User>> GetUsersAsync(bool includeDeleted = false)
    {
        if (!includeDeleted) return await Task.FromResult(_context.Users.Where(u => u.IsActive).ToList());
        return await Task.FromResult(_context.Users.ToList());
    }

    public async Task<Model.Entities.User> CreateUserAsync(Model.Entities.User user)
    {
        var trackedUser = await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        return trackedUser.Entity;
    }

    public async Task<Model.Entities.User?> GetUserAsync(string userId)
    {
        return await Task.FromResult(_context.Users.ToList().Find(u => u.Id == userId));
    }

    public async Task<Model.Entities.User?> GetUserByEmailAsync(string email)
    {
        return await Task.FromResult(_context.Users.ToList()
            .Find(u => u.Email.ToLowerInvariant() == email.ToLowerInvariant()));
    }

    public async Task<Model.Entities.User> SaveUserAsync(Model.Entities.User user)
    {
        var trackedUser = _context.Users.Update(user);
        await _context.SaveChangesAsync();

        return trackedUser.Entity;
    }
}