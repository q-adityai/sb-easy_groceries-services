using EasyGroceries.Common.DependencyInjection;
using EasyGroceries.Order;
using EasyGroceries.Order.Model.Context;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(Program))]

namespace EasyGroceries.Order;

public class Program : FunctionsStartup
{
    public override void Configure(IFunctionsHostBuilder builder)
    {
        builder.AddCosmosDb<OrderContext>();
        
        builder.Services.AddLogging();
        builder.AddMessaging();


        builder.Services.AddAutoMapper(typeof(Program));
    }
}