using System;
using EasyGroceries.Common.Entities;

namespace EasyGroceries.Inventory.Model.Entities;

public class Category
{
    public string Id { get; set; } = $"{Constants.CategoryPrefix}{Guid.NewGuid()}";
    public string Name { get; set; } = null!;
    public bool IsActive { get; set; }
    public bool IncludeInDelivery { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}