using Newtonsoft.Json;

namespace EasyGroceries.Common.Extensions;

public static class ObjectExtensions
{
    public static T? Clone<T>(this T input)
    {
        if (input is null)
            return default;
        
        return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(input));
    }
}