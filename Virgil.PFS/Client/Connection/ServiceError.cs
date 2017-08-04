using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Virgil.PFS.Client.Connection
{
    [DataContract]
    internal class ServiceError
    {
        [DataMember(Name = "code")]
        public int ErrorCode { get; set; }
    }
}
