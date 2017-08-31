using System.Net.Http;
using Virgil.PFS.Client.Connection;

namespace Virgil.PFS
{
    internal interface IResponseErrorHandler
    {
        void ThrowServiceException(IResponse response);

        void ThrowBaseException(IResponse response);
    }
}