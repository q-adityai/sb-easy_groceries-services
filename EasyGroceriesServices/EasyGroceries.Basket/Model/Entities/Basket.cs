using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using EasyGroceries.Common.Entities;
using EasyGroceries.Common.Enums;

namespace EasyGroceries.Basket.Model.Entities;

public class Basket
{
    [Key] public string Id { get; set; }
    public string UserId { get; set; } = null!;
    public BasketStatus Status { get; set; } = BasketStatus.Empty;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public Basket()
    {
        Id = $"{Constants.BasketPrefix}{Guid.NewGuid()}";
    }
}