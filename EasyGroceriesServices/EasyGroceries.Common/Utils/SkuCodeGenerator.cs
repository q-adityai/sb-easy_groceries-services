using shortid;
using shortid.Configuration;

namespace EasyGroceries.Common.Utils;

public static class SkuCodeGenerator
{
    public static string Generate()
    {
        return ShortId.Generate(new GenerationOptions(true, false, 8));
    }
}