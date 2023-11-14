using EasyGroceries.Basket;
using EasyGroceries.Basket.Model.Context;
using EasyGroceries.Basket.Repositories;
using EasyGroceries.Basket.Repositories.Interfaces;
using EasyGroceries.Common.Configuration;
using EasyGroceries.Common.DependencyInjection;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

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

        
        
        builder.Services.TryAddSingleton<IUserRepository, UserRepository>();
        builder.Services.TryAddSingleton<IProductRepository, ProductRepository>();
        builder.Services.TryAddSingleton<IBasketRepository, BasketRepository>();
    }
}