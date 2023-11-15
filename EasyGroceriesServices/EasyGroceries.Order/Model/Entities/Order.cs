using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using EasyGroceries.Common.Entities;

namespace EasyGroceries.Order.Model.Entities;

public class Order
{
    [Key]public string Id { get; set; }
    public string BasketId { get; set; }
    public string UserId { get; set; }
    public Money OrderValue { get; set; }
    public DefaultAddress DeliveryAddress { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public Order()
    {
        Id = $"{Constants.OrderPrefix}{Guid.NewGuid()}";
    }
}
