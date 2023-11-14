using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using AutoMapper;
using Xunit;

namespace EasyGroceries.Basket.Tests.Mapping;

public class MappingTests
{
    private readonly MapperConfiguration _mapperConfiguration;

    public MappingTests()
    {
        var types = GetLoadableTypes(typeof(Program).Assembly);

        var apiProfiles = types
            .Where(t => typeof(Profile).IsAssignableFrom(t))
            .ToList();

        _mapperConfiguration = new MapperConfiguration(cfg =>
            {
                foreach (var profile in apiProfiles) cfg.AddProfile(profile);
            }
        );
    }

    [Fact]
    public void VerifyMappings()
    {
        _mapperConfiguration.AssertConfigurationIsValid();
    }

    [ExcludeFromCodeCoverage]
    private static IEnumerable<Type?> GetLoadableTypes(Assembly assembly)
    {
        if (assembly == null) throw new ArgumentNullException(nameof(assembly));

        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException e)
        {
            return e.Types.Where(t => t != null);
        }
    }
}