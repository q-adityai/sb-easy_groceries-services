using EasyGroceries.Basket;
using EasyGroceries.Basket.Model.Context;
using EasyGroceries.Common.DependencyInjection;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(Program))]

namespace EasyGroceries.Basket;

public class Program : FunctionsStartup
{
    public override void Configure(IFunctionsHostBuilder builder)
    {
        builder.AddCosmosDb<BasketContext>();
        
        builder.Services.AddLogging();
        builder.AddMessaging();


        builder.Services.AddAutoMapper(typeof(Program));
    }
}