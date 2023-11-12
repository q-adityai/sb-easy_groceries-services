using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Extensions.Primitives;
using Moq;
using Newtonsoft.Json;

namespace EasyGroceries.Tests.Common.Utils;

public static class MockHttpRequest
{
    public static Mock<HttpRequest> Create(PathString? path = null, IQueryCollection? query = null, object? body = null)
    {
        var request = new Mock<HttpRequest>();

        path ??= new PathString();
        request.Setup(x => x.Path).Returns(path.Value);

        query ??= new QueryCollection(new Dictionary<string, StringValues>());
        request.Setup(x => x.Query).Returns(query);

        if (body != null)
        {
            var ms = new MemoryStream();
            var sw = new StreamWriter(ms);
 
            var json = JsonConvert.SerializeObject(body);
 
            sw.Write(json);
            sw.Flush();
 
            ms.Position = 0;
            request.Setup(x => x.Body).Returns(ms);
        }

        return request;
    }
}