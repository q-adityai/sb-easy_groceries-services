using EasyGroceries.Common.Entities;

namespace EasyGroceries.Basket.Model.Entities;

public class BasketProduct
{
    //public Product Product { get; set; } = null!;
    public string ProductId { get; set; } = null!;

    public string Sku { get; set; } = null!;

    public string CategoryName { get; set; } = null!;

    public bool IncludeInDelivery { get; set; }

    public string Name { get; set; } = null!;

    public string Description { get; set; } = null!;

    public Money Price { get; set; } = null!;
    public Money DiscountedPrice { get; set; } = null!;

    public int DiscountPercentInMinorUnits { get; set; }

    public bool DiscountApplicable { get; set; }
    public long Quantity { get; set; } = 0;
}