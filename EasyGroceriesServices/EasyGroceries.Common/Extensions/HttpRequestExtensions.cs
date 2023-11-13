using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace EasyGroceries.Common.Extensions;

public static class HttpRequestExtensions
{
    public static async Task<T> GetBody<T>(this HttpRequest httpRequest)
    {
        var requestBody = await new StreamReader(httpRequest.Body).ReadToEndAsync();
        return JsonConvert.DeserializeObject<T>(requestBody);
    }
}