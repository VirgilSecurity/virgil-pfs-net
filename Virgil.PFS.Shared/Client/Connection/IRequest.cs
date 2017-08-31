using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Virgil.PFS.Client.Connection
{
    /// <summary>
    /// Represent a generic HTTP request 
    /// </summary>
    internal interface IRequest
    {
        /// <summary>
        /// Gets the endpoint. Does not include server base address
        /// </summary>
        string Endpoint { get; }

        /// <summary>
        /// Gets the request method.
        /// </summary>
        HttpRequestMethod Method { get; }

        /// <summary>
        /// Gets the http headers.
        /// </summary>
        IDictionary<string, string> Headers { get; }

        /// <summary>
        /// Gets the requests body.
        /// </summary>
        string Body { get; }
    }
}
