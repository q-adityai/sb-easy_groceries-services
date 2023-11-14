using EasyGroceries.Common.Configuration;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EasyGroceries.Common.DependencyInjection;

public static class CosmosDbExtensions
{
    public static IFunctionsHostBuilder AddCosmosDb<T>(this IFunctionsHostBuilder builder) where T: DbContext
    {
        builder.Services.AddDbContext<T>(options =>
        {
            var configuration = builder.GetContext().Configuration;
            var cosmosDbOptionsSection = configuration.GetSection(CosmosDbOptions.SectionName);
            var cosmosDbOptions = cosmosDbOptionsSection.Get<CosmosDbOptions>();
            
            options.UseCosmos(cosmosDbOptions.Uri, cosmosDbOptions.Key, cosmosDbOptions.DatabaseName);
            //options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
        }, ServiceLifetime.Singleton, ServiceLifetime.Singleton);
        return builder;
    }
}