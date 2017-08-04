using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Virgil.PFS.Client.Connection;

namespace Virgil.PFS.Client
{
    /// <summary>
    /// Extensions to help construct http requests
    /// </summary>
    internal static class RequestExtensions
    {
        /// <summary>
        /// Sets the request enpoint
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="endpoint">The endpoint.</param>
        /// <returns><see cref="IRequest"/></returns>
        public static HttpRequest WithEndpoint(this HttpRequest request, string endpoint)
        {
            request.Endpoint = endpoint;
            return request;
        }

        /// <summary>
        /// Withes the body.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="body">The body.</param>
        /// <returns><see cref="IRequest"/></returns>
        public static HttpRequest WithBody(this HttpRequest request, object body)
        {
            request.Body = JsonSerializer.Serialize(body);
            return request;
        }
    }
}
