using EasyGroceries.Common.Enums;

namespace EasyGroceries.Common.Entities;

public class Address
{
    public string Id { get; set; } = $"{Constants.AddressPrefix}{Guid.NewGuid()}";
    public string Line1 { get; set; } = null!;
    public string? Line2 { get; set; }
    public string? Line3 { get; set; }
    public string? City { get; set; }
    public string? County { get; set; }
    public string Postcode { get; set; } = null!;
    public string? Country { get; set; }
    public CountryCode CountryCode { get; set; }
}