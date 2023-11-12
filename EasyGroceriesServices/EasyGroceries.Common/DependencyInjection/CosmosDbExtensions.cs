using EasyGroceries.Common.Configuration;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace EasyGroceries.Common.DependencyInjection;

public static class CosmosDbExtensions
{
    public static IFunctionsHostBuilder AddCosmosDb(this IFunctionsHostBuilder builder)
    {
        var configuration = builder.GetContext().Configuration;
        var cosmosDbOptionsSection = configuration.GetSection(CosmosDbOptions.SectionName);
        builder.Services.Configure<CosmosDbOptions>(cosmosDbOptionsSection);
        return builder;
    }
}

