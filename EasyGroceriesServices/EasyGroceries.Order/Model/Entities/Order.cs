using System;
using System.Collections.Generic;
using EasyGroceries.Common.Entities;

namespace EasyGroceries.Order.Model.Entities;

public class Order
{
    public string Id { get; set; } = $"{Constants.OrderPrefix}{Guid.NewGuid()}";
    public string BasketId { get; set; }
    public string UserId { get; set; }
    public Money BasketValue { get; set; }
    public List<OrderItem> Items { get; set; }
    public Address DeliveryAddress { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
