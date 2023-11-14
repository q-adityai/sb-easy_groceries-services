using AutoFixture;
using AutoFixture.AutoMoq;
using EasyGroceries.User.Model.Context;
using EasyGroceries.User.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EasyGroceries.User.Tests.Repositories;

public class UserRepositoryTests
{
    private readonly IFixture _fixture = new Fixture().Customize(new AutoMoqCustomization());
    
    private UserContext CreateInventoryContext(string databaseName)
    {
        var dbOptions = new DbContextOptionsBuilder<UserContext>();
        dbOptions.UseInMemoryDatabase(databaseName);
        return new UserContext(dbOptions.Options);
    }

    [Fact]
    public async Task GetUsersAsync_Include_Deleted_False_Returns_Active_User()
    {
        //Arrange
        await using var userContext = CreateInventoryContext(nameof(GetUsersAsync_Include_Deleted_False_Returns_Active_User));
        var service = new UserRepository(userContext);

        await userContext.Users.AddRangeAsync(_fixture.Build<Model.Entities.User>().With(x => x.IsActive, false).CreateMany(5).ToList());
        await userContext.Users.AddRangeAsync(_fixture.Build<Model.Entities.User>().With(x => x.IsActive, true).CreateMany(3).ToList());
        await userContext.SaveChangesAsync();
        
        //Act
        var result = await service.GetUsersAsync();

        //Assert
        result.Should().NotBeEmpty();
        result.Count.Should().Be(3);
    }
    
    [Fact]
    public async Task GetUsersAsync_Include_Deleted_False_Returns_No_Active_User()
    {
        //Arrange
        await using var userContext = CreateInventoryContext(nameof(GetUsersAsync_Include_Deleted_False_Returns_No_Active_User));
        var service = new UserRepository(userContext);

        await userContext.Users.AddRangeAsync(_fixture.Build<Model.Entities.User>().With(x => x.IsActive, false).CreateMany(5).ToList());
        await userContext.SaveChangesAsync();
        
        //Act
        var result = await service.GetUsersAsync();

        //Assert
        result.Should().BeEmpty();
        result.Count.Should().Be(0);
    }
    
    [Fact]
    public async Task GetUsersAsync_Include_Deleted_True_Returns_Active_User()
    {
        //Arrange
        await using var userContext = CreateInventoryContext(nameof(GetUsersAsync_Include_Deleted_True_Returns_Active_User));
        var service = new UserRepository(userContext);

        await userContext.Users.AddRangeAsync(_fixture.Build<Model.Entities.User>().With(x => x.IsActive, false).CreateMany(5).ToList());
        await userContext.Users.AddRangeAsync(_fixture.Build<Model.Entities.User>().With(x => x.IsActive, true).CreateMany(3).ToList());
        await userContext.SaveChangesAsync();
        
        //Act
        var result = await service.GetUsersAsync(true);

        //Assert
        result.Should().NotBeEmpty();
        result.Count.Should().Be(8);
    }
    
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task GetUsersAsync_Returns_No_Users(bool includeDeletedUsers)
    {
        //Arrange
        await using var userContext = CreateInventoryContext(nameof(GetUsersAsync_Returns_No_Users));
        var service = new UserRepository(userContext);

        //Act
        var result = await service.GetUsersAsync(includeDeletedUsers);

        //Assert
        result.Should().BeEmpty();
        result.Count.Should().Be(0);
    }

    [Fact]
    public async Task CreateUserAsync_Creates_User()
    {
        //Arrange
        var user = _fixture.Create<Model.Entities.User>();
        
        await using var userContext = CreateInventoryContext(nameof(CreateUserAsync_Creates_User));
        var service = new UserRepository(userContext);

        //Act
        var result = await service.CreateUserAsync(user);

        //Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(user.Id);
        result.BillingAddresses[0].CreatedAt.Should().Be(user.BillingAddresses[0].CreatedAt);
        result.BillingAddresses[0].LastModifiedAt.Should().Be(user.BillingAddresses[0].LastModifiedAt);
    }

    [Fact]
    public async Task GetUserAsync_Returns_User()
    {
        var setupUser = _fixture.Create<Model.Entities.User>();
        
        await using var userContext = CreateInventoryContext(nameof(GetUserAsync_Returns_User));
        var service = new UserRepository(userContext);

        await userContext.Users.AddAsync(setupUser);
        await userContext.SaveChangesAsync();

        //Act
        var result = await service.GetUserAsync(setupUser.Id);
        
        //Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(setupUser.Id);
        result.BillingAddresses[0].CreatedAt.Should().Be(setupUser.BillingAddresses[0].CreatedAt);
        result.BillingAddresses[0].LastModifiedAt.Should().Be(setupUser.BillingAddresses[0].LastModifiedAt);
    }
    
    [Fact]
    public async Task GetUserByEmailAsync_Returns_User()
    {
        var setupUser = _fixture.Create<Model.Entities.User>();
        
        await using var userContext = CreateInventoryContext(nameof(GetUserByEmailAsync_Returns_User));
        var service = new UserRepository(userContext);

        await userContext.Users.AddAsync(setupUser);
        await userContext.SaveChangesAsync();

        //Act
        var result = await service.GetUserByEmailAsync(setupUser.Email);
        
        //Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(setupUser.Id);
        result.BillingAddresses[0].CreatedAt.Should().Be(setupUser.BillingAddresses[0].CreatedAt);
        result.BillingAddresses[0].LastModifiedAt.Should().Be(setupUser.BillingAddresses[0].LastModifiedAt);
    }
    
    [Fact]
    public async Task SaveUserAsync_Updates_User()
    {
        //Arrange
        var setupUser = _fixture.Create<Model.Entities.User>();
        
        await using var userContext = CreateInventoryContext(nameof(SaveUserAsync_Updates_User));
        var service = new UserRepository(userContext);
        
        await userContext.Users.AddAsync(setupUser);
        await userContext.SaveChangesAsync();

        //Act
        setupUser.FirstName = "Changed FirstName";
        var result = await service.SaveUserAsync(setupUser);

        //Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(setupUser.Id);
        result.FirstName.Should().Be("Changed FirstName");
        result.BillingAddresses[0].CreatedAt.Should().Be(setupUser.BillingAddresses[0].CreatedAt);
        result.BillingAddresses[0].LastModifiedAt.Should().Be(setupUser.BillingAddresses[0].LastModifiedAt);
    }
}