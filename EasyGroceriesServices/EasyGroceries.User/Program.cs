using EasyGroceries.Common.DependencyInjection;
using EasyGroceries.User;
using EasyGroceries.User.Model.Context;
using EasyGroceries.User.Repositories;
using EasyGroceries.User.Repositories.Interfaces;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

[assembly: FunctionsStartup(typeof(Program))]

namespace EasyGroceries.User;

public class Program : FunctionsStartup
{
    public override void Configure(IFunctionsHostBuilder builder)
    {
        builder.Services.AddLogging();

        builder.AddCosmosDb();
        builder.AddMessaging();


        builder.Services.AddAutoMapper(typeof(Program));

        builder.Services.TryAddSingleton<UserContext>();
        builder.Services.TryAddSingleton<IUserRepository, UserRepository>();
    }
}