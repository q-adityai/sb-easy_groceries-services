using EasyGroceries.Common.Enums;

namespace EasyGroceries.Common.Entities;

public class Money
{
    public Currency Currency { get; set; } = Currency.Unknown;
    public long AmountInMinorUnits { get; set; }
}