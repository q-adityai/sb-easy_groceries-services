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
        builder.Services.AddLogging();


        builder.AddCosmosDb();
        builder.AddMessaging();


        builder.Services.AddAutoMapper(typeof(Program));

        builder.Services.AddDbContext<BasketContext>(options =>
        {
            var configuration = builder.GetContext().Configuration;
            var cosmosDbOptionsSection = configuration.GetSection(CosmosDbOptions.SectionName);
            var cosmosDbOptions = cosmosDbOptionsSection.Get<CosmosDbOptions>();
            
            options.UseCosmos(cosmosDbOptions.Uri, cosmosDbOptions.Key, cosmosDbOptions.DatabaseName);
            options.EnableSensitiveDataLogging(true);
        }, ServiceLifetime.Singleton, ServiceLifetime.Singleton);
        
        // builder.Services.TryAddSingleton<BasketContext>();
        builder.Services.TryAddSingleton<IUserRepository, UserRepository>();
        builder.Services.TryAddSingleton<IProductRepository, ProductRepository>();
        builder.Services.TryAddSingleton<IBasketRepository, BasketRepository>();
    }
}