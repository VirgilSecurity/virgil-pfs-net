using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Virgil.PFS.Client.Connection
{
    internal static class HttpMethodExtensions
    {
        public static HttpMethod GetMethod(this HttpRequestMethod requestMethod)
        {
            switch (requestMethod)
            {
                case HttpRequestMethod.Get: return HttpMethod.Get;
                case HttpRequestMethod.Post: return HttpMethod.Post;
                case HttpRequestMethod.Put: return HttpMethod.Put;
                case HttpRequestMethod.Delete: return HttpMethod.Delete;
                default:
                    throw new ArgumentOutOfRangeException(nameof(requestMethod));
            }
        }
    }
}
