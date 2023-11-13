using EasyGroceries.Common.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace EasyGroceries.User.Model.Context;

public class UserContext : DbContext
{
    private readonly CosmosDbOptions _options;

    public UserContext(IOptions<CosmosDbOptions> options)
    {
        _options = options.Value;
    }

    public DbSet<Entities.User> Users { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseCosmos(_options.Uri, _options.Key, _options.DatabaseName);
        base.OnConfiguring(optionsBuilder);
    }
}