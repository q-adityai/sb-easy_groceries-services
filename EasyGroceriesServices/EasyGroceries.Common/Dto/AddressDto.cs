using System.ComponentModel.DataAnnotations;
using EasyGroceries.Common.Enums;

namespace EasyGroceries.Common.Dto;

public class AddressDto
{
    public string? Id { get; set; }

    [Required] public string Line1 { get; set; } = null!;

    public string? Line2 { get; set; }
    public string? Line3 { get; set; }
    public string? City { get; set; }
    public string? County { get; set; }

    [Required] public string Postcode { get; set; } = null!;

    public string? Country { get; set; }

    [Required] public CountryCode CountryCode { get; set; }
}