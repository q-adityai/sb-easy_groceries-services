using AutoFixture;
using AutoFixture.AutoMoq;
using EasyGroceries.Inventory.Model.Context;
using EasyGroceries.Inventory.Model.Entities;
using EasyGroceries.Inventory.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EasyGroceries.Inventory.Tests.Repositories;

public class ProductRepositoryTests
{
    private readonly IFixture _fixture = new Fixture().Customize(new AutoMoqCustomization());

    [Fact]
    public async Task GetAllApplicableProductsAsync_Returns_Products()
    {
        //Arrange
        await using var inventoryContext = CreateInventoryContext(nameof(GetAllApplicableProductsAsync_Returns_Products));
        var service = new ProductRepository(inventoryContext);
        
        inventoryContext.Products.AddRange(_fixture.Build<Product>()
            .With(x => x.ValidFrom, DateTimeOffset.UtcNow.AddDays(-10))
            .With(x => x.ValidTo, DateTimeOffset.MaxValue)
            .CreateMany(5));
        await inventoryContext.SaveChangesAsync();
        
        //Act
        var result = await service.GetAllApplicableProductsAsync();

        //Assert
        result.Should().NotBeEmpty();
        result.Count.Should().Be(5);
    }
    
    [Fact]
    public async Task GetProductsAsync_Returns_Products()
    {
        //Arrange
        await using var inventoryContext = CreateInventoryContext(nameof(GetProductsAsync_Returns_Products));
        var service = new ProductRepository(inventoryContext);

        inventoryContext.Products.AddRange(_fixture.Build<Product>()
            .CreateMany(5));
        await inventoryContext.SaveChangesAsync();
        
        //Act
        var result = await service.GetProductsAsync();

        //Assert
        result.Should().NotBeEmpty();
        result.Count.Should().Be(5);
    }
    
    [Fact]
    public async Task GetProductByNameAsync_Returns_Product()
    {
        //Arrange
        await using var inventoryContext = CreateInventoryContext(nameof(GetProductByNameAsync_Returns_Product));
        var service = new ProductRepository(inventoryContext);

        inventoryContext.Products.Add(_fixture.Build<Product>()
            .With(x => x.Name, "some name")
            .Create());
        await inventoryContext.SaveChangesAsync();
        
        //Act
        var result = await service.GetProductByNameAsync("some name");

        //Assert
        result.Should().NotBeNull();
    }
    
    [Fact]
    public async Task GetProductByNameAsync_Returns_Null()
    {
        //Arrange
        await using var inventoryContext = CreateInventoryContext(nameof(GetProductByNameAsync_Returns_Null));
        var service = new ProductRepository(inventoryContext);

        inventoryContext.Products.Add(_fixture.Build<Product>()
            .With(x => x.Name, "some name")
            .Create());
        await inventoryContext.SaveChangesAsync();
        
        //Act
        var result = await service.GetProductByNameAsync("some name2");

        //Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateProductAsync_Creates_Product()
    {
        //Arrange
        var product = _fixture.Create<Product>();
        
        await using var inventoryContext = CreateInventoryContext(nameof(CreateProductAsync_Creates_Product));
        var service = new ProductRepository(inventoryContext);
        
        //Act
        var response = await service.CreateProductAsync(product);

        //Assert
        response.Should().NotBeNull();
        response.Id.Should().Be(product.Id);
        response.DiscountApplicable.Should().Be(product.DiscountApplicable);
        response.ValidFrom.Should().Be(product.ValidFrom);
        response.ValidTo.Should().Be(product.ValidTo);
    }

    private InventoryContext CreateInventoryContext(string databaseName)
    {
        var dbOptions = new DbContextOptionsBuilder<InventoryContext>();
        dbOptions.UseInMemoryDatabase(databaseName);
        return new InventoryContext(dbOptions.Options);
    }
}