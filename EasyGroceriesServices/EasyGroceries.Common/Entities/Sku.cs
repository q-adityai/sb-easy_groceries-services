using shortid.Configuration;

namespace EasyGroceries.Common.Entities;

public class Sku
{
    public string Code { get; private set; } = null!;

    private Sku()
    {
        
    }

    public static Sku Generate()
    {
        return new Sku() { Code = shortid.ShortId.Generate(new GenerationOptions(true, false, 8)) };
    }
}