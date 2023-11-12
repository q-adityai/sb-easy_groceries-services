using EasyGroceries.Inventory.Model.Context;
using EasyGroceries.Inventory.Repositories.Interfaces;

namespace EasyGroceries.Inventory.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly InventoryContext _context;

    public CategoryRepository(InventoryContext context)
    {
        _context = context;
        _context.Database.EnsureDeleted();
        _context.Database.EnsureCreated();
    }
}