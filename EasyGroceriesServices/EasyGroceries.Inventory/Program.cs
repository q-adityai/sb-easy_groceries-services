using EasyGroceries.Common.DependencyInjection;
using EasyGroceries.Inventory.Model.Context;
using EasyGroceries.Inventory.Repositories;
using EasyGroceries.Inventory.Repositories.Interfaces;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

[assembly: FunctionsStartup(typeof(EasyGroceries.Inventory.Program))]
namespace EasyGroceries.Inventory;

public class Program : FunctionsStartup
{
    public override void Configure(IFunctionsHostBuilder builder)
    {
        builder.Services.AddLogging();
        
        
        builder.AddCosmosDb();
        builder.AddMessaging();
        
        builder.Services.AddAutoMapper(typeof(Program));
        
        builder.Services.TryAddSingleton<InventoryContext>();
        builder.Services.TryAddSingleton<ICategoryRepository, CategoryRepository>();
        builder.Services.TryAddSingleton<IProductRepository, ProductRepository>();
    }
}