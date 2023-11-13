using Newtonsoft.Json;

namespace EasyGroceries.Common.Extensions;

public static class ObjectExtensions
{
    public static T Clone<T>(this T input)
    {
        return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(input));
    }
}