using System.Linq;
using Microsoft.AspNetCore.Http;

namespace Eventus.Samples.Web
{
    internal static class RequestExtensions
    {
        internal static bool IsForView(this HttpRequest request)
        {
            return request.GetTypedHeaders().Accept.Any(x => x.ToString() == "text/html");
        }
    }
}