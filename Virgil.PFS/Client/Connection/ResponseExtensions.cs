using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Virgil.PFS.Client.Connection;

namespace Virgil.PFS.Client
{
    internal static class ResponseExtensions
    {
        public static TResult Parse<TResult>(this IResponse response)
        {
            return JsonSerializer.Deserialize<TResult>(response.Body);
        }
    }
}
