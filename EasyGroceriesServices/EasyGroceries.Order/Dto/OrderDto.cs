using System.Collections.Generic;
using EasyGroceries.Common.Dto;
using EasyGroceries.Common.Entities;

namespace EasyGroceries.Order.Dto;

public class OrderDto
{
    public string OrderId { get; set; }
    public string UserId { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public Money BasketValue { get; set; }
    public List<OrderItemDto> Products { get; set; }
    public DefaultAddress DeliveryAddress { get; set; }
}
