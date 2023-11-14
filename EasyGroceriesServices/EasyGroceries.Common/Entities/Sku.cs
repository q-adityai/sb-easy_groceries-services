using shortid;
using shortid.Configuration;

namespace EasyGroceries.Common.Entities;

public class Sku
{
    private Sku()
    {
    }

    public string Code { get; private set; } = null!;

    public static Sku Generate()
    {
        return new Sku { Code = ShortId.Generate(new GenerationOptions(true, false, 8)) };
    }
}