namespace EasyGroceries.Basket.Model.Entities;

public class BasketProduct
{
    public Product Product { get; set; } = null!;
    public long Quantity { get; set; } = 0;
}