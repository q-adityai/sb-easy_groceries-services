using EasyGroceries.Common.Enums;

namespace EasyGroceries.Order.Model.Entities;

public class Address
{
    public string Line1 { get; set; }
    public string? Line2 { get; set; }
    public string? Line3 { get; set; }
    public string? City { get; set; }
    public string? County { get; set; }
    public string Postcode { get; set; }
    public string? Country { get; set; }
    public CountryCode CountryCode { get; set; }
}