using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Virgil.PFS.Client.Connection;
using Virgil.SDK.Exceptions;

namespace Virgil.PFS.Client
{
    internal class PfsServiceResponseErrorHandler : IResponseErrorHandler
    {
        /// <summary>
        /// The error code to message mapping dictionary
        /// </summary>
        protected Dictionary<int, string> Errors = new Dictionary<int, string>
        {
            [30000] = "JSON specified as a request is invalid",
            [30001] = "Request snapshot invalid",
            [30138] = "Virgil Card with the same fingerprint exists already",
            [30140] = "SCR sign validation failed(recipient)",
            [60000] = "Card scope should be application",
            [60010] = "Maximum number of OTCs 100",
            [60011] = "Exceeded number of items in request",
            [60100] = "Unsupported public key type"
        };

        public void ThrowServiceException(IResponse response)
        {
            var serviceError = this.ReadServiceError(response.Body);
            string errorMessage;

            if (this.Errors.TryGetValue(serviceError.ErrorCode, out errorMessage))
                throw new VirgilServiceException(serviceError.ErrorCode, errorMessage);

            if (serviceError.ErrorCode == 0)
                ThrowBaseException(response);
            else
            {
                errorMessage = $"Undefined exception: {serviceError.ErrorCode}; Http status: {response.StatusCode}";
                throw new VirgilServiceException(serviceError.ErrorCode, errorMessage);
            }
        }


        public void ThrowBaseException(IResponse response)
        {
            string errorMessage;
            switch (response.StatusCode)
            {
                case 400: errorMessage = "Request Error"; break;
                case 401: errorMessage = "Authorization Error"; break;
                case 403: errorMessage = "Forbidden"; break;
                case 404: errorMessage = "Entity Not Found"; break;
                case 405: errorMessage = "Method Not Allowed"; break;
                case 500: errorMessage = "Internal Server Error"; break;
                default:
                    errorMessage = $"Undefined exception (Http Status Code: {response.StatusCode})";
                    break;
            }

            throw new VirgilServiceException(0, errorMessage);
        }

        private ServiceError ReadServiceError(string content)
        {
            return JsonConvert.DeserializeObject<ServiceError>(content);
        }
    }
}
