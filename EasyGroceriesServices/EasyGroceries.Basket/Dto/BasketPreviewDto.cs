using System.Collections.Generic;
using EasyGroceries.Common.Dto;
using EasyGroceries.Common.Entities;

namespace EasyGroceries.Basket.Dto;

public class BasketPreviewDto
{
    public string BasketId { get; set; }
    public string UserId { get; set; }
    public Money BasketValue { get; set; }
    public List<BasketProductPreviewDto> Products { get; set; }
    public AddressDto DeliveryAddress { get; set; }
}
