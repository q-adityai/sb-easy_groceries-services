using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using EasyGroceries.Common.Entities;
using EasyGroceries.Common.Enums;

namespace EasyGroceries.Basket.Model.Entities;

public class Basket
{
    [Required]
    public string Id { get; set; } = $"{Constants.BasketPrefix}{Guid.NewGuid()}";
    [Required]
    public string UserId { get; set; } = null!;
    [Required]
    public List<BasketProduct> Products { get; set; } = new();

    public BasketStatus Status { get; set; } = BasketStatus.Empty;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}