using System;
using System.Collections.Generic;
using Virgil.PFS.Client.Connection;

namespace Virgil.PFS.Client.Connection
{
    /// <summary>
    /// <see cref="IRequest" /> default implementation"/>
    /// </summary>
    /// <seealso cref="IRequest" />
    internal class HttpRequest : IRequest
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HttpRequest"/> class.
        /// </summary>
        public HttpRequest()
        {
            this.Headers = new Dictionary<string, string>();
        }

        /// <summary>
        /// Gets the endpoint. Does not include server base address
        /// </summary>
        public string Endpoint { get; set; }

        /// <summary>
        /// Gets the requests body.
        /// </summary>
        public string Body { get; set; }

        /// <summary>
        /// Gets the http headers.
        /// </summary>
        public IDictionary<string, string> Headers { get; set; }

        /// <summary>
        /// Gets the request method.
        /// </summary>
        public HttpRequestMethod Method { get; set; }

        internal static HttpRequest Create(HttpRequestMethod method)
        {
            return new HttpRequest { Method = method };
        }
    }
}