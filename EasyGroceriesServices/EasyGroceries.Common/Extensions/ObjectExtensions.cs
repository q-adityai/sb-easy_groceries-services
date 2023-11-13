using Newtonsoft.Json;

namespace EasyGroceries.Common.Extensions;

public static class ObjectExtensions
{
    public static string ToString(this object input)
    {
        return JsonConvert.SerializeObject(input);
    }
}