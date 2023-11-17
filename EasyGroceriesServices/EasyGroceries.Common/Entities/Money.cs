using EasyGroceries.Common.Enums;
using Microsoft.EntityFrameworkCore;

namespace EasyGroceries.Common.Entities;

[Owned]
public class Money
{
    public Currency Currency { get; set; } = Currency.Unknown;
    public long AmountInMinorUnits { get; set; }
}