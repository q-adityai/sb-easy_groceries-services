namespace EasyGroceries.Basket.Configuration;

public class BasketApiOptions
{
    public static readonly string SectionName = "BasketApiOptions";
    public int DefaultDiscountPercentInMinorUnits { get; set; }
}