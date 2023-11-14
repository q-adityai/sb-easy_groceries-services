using EasyGroceries.Common.Configuration;
using EasyGroceries.Common.DependencyInjection;
using EasyGroceries.Inventory;
using EasyGroceries.Inventory.Model.Context;
using EasyGroceries.Inventory.Repositories;
using EasyGroceries.Inventory.Repositories.Interfaces;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

[assembly: FunctionsStartup(typeof(Program))]

namespace EasyGroceries.Inventory;

public class Program : FunctionsStartup
{
    public override void Configure(IFunctionsHostBuilder builder)
    {
        builder.Services.AddLogging();
        builder.AddMessaging();

        builder.Services.AddAutoMapper(typeof(Program));
        
        builder.AddCosmosDb<InventoryContext>();

        builder.Services.TryAddSingleton<IProductRepository, ProductRepository>();
    }
}