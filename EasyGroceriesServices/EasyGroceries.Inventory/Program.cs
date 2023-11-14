using EasyGroceries.Common.DependencyInjection;
using EasyGroceries.Inventory;
using EasyGroceries.Inventory.Model.Context;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

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
    }
}