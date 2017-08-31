using System;
using System.Collections.Generic;
using Virgil.PFS.Client.Connection;

namespace Virgil.PFS.Client.Connection
{
    /// <summary>
    /// <see cref="IResponse"/> default implementation
    /// </summary>
    internal class HttpResponse : IResponse
    {
        /// <summary>
        /// Raw response body.
        /// </summary>
        public string Body { get; set; }

        /// <summary>
        /// Information about the API.
        /// </summary>
        public IReadOnlyDictionary<string, string> Headers { get; set; }

        public bool IsSuccessStatuseCode()
        {
            return (this.StatusCode >= 200 && this.StatusCode <= 299);
        }

        /// <summary>
        /// The response status code.
        /// </summary>
        public int StatusCode { get; set; }
    }
}