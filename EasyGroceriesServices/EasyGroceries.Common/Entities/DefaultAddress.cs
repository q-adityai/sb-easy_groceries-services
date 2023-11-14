using System;
using EasyGroceries.Common.Enums;
using Microsoft.EntityFrameworkCore;

namespace EasyGroceries.Common.Entities;

[Owned]
public class DefaultAddress
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