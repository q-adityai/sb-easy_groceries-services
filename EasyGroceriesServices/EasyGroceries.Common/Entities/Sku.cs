using Microsoft.EntityFrameworkCore;

namespace EasyGroceries.Common.Entities;
[Owned]
public class Sku
{
    public string Code { get; set; } = null!;
}