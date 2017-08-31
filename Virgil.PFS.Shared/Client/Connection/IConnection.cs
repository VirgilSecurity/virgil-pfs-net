using System;
using System.Threading.Tasks;

namespace Virgil.PFS.Client.Connection
{
    internal interface IConnection
    {
        /// <summary>
        /// Base address for the connection.
        /// </summary>
        Uri BaseURL { get; }

        /// <summary>
        /// Sends an HTTP request to the API.
        /// </summary>
        /// <param name="request">The HTTP request details.</param>
        Task<IResponse> SendAsync(IRequest request);
    }
}