using System;
using EasyGroceries.Common.Entities;

namespace EasyGroceries.Inventory.Model.Entities;

public class Product
{
    public string Id { get; set; }
    public Sku Sku { get; set; }
    public Category Category { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public Money Price { get; set; }
    public long StockQuantity { get; set; }
    public DateTimeOffset ValidFrom { get; set; }
    public DateTimeOffset ValidTo { get; set; }
}