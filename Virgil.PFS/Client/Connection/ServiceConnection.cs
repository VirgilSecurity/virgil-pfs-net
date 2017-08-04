using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Virgil.SDK.Exceptions;

namespace Virgil.PFS.Client.Connection
{
    internal class ServiceConnection : IConnection
    {
        /// <summary>
        /// The access token header name
        /// </summary>
        protected const string AccessTokenHeaderName = "Authorization";


        /// <summary>
        // Base URL for API requests. Defaults to the public Virgil API, but 
        // can be set to a domain endpoint to use with Virgil Enterprise. 
        /// </summary>
        /// <remarks>
        /// BaseURL should always be specified with a trailing slash.
        /// </remarks>
        public Uri BaseURL { get; set; }

        /// <summary>
        /// Gets or sets the Access Token.
        /// </summary>
        public string AccessToken { get; set; }


        /// <summary>
        /// Sends an HTTP request to the API.
        /// </summary>
        /// <param name="request">The HTTP request details.</param>
        /// <returns>Response</returns>
        public virtual async Task<IResponse> SendAsync(IRequest request)
        {
            using (var httpClient = new HttpClient())
            {
                var nativeRequest = this.GetNativeRequest(request);
                var nativeResponse = await httpClient.SendAsync(nativeRequest).ConfigureAwait(false);
                var content = nativeResponse.Content.ReadAsStringAsync().Result;

                var response = new HttpResponse
                {
                    Body = content,
                    Headers = nativeResponse.Headers.ToDictionary(it => it.Key, it => it.Value.FirstOrDefault()),
                    StatusCode = (int) nativeResponse.StatusCode
                };

                return response;
            }
        }

        /// <summary>
        /// Produces native HTTP request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>HttpRequestMessage</returns>
        protected virtual HttpRequestMessage GetNativeRequest(IRequest request)
        {
            var message = new HttpRequestMessage(request.Method.GetMethod(),
                new Uri(this.BaseURL, request.Endpoint));

            if (request.Headers != null)
            {
                if (this.AccessToken != null)
                {
                    message.Headers.TryAddWithoutValidation(AccessTokenHeaderName, $"VIRGIL {this.AccessToken}");
                }

                foreach (var header in request.Headers)
                {
                    message.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            }

            if (request.Method != HttpRequestMethod.Get)
            {
                message.Content = new StringContent(request.Body, Encoding.UTF8, "application/json");
            }

            return message;
        }
    }
}
