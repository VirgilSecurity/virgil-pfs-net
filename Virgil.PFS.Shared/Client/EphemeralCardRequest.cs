using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Virgil.PFS.Client.Models;

namespace Virgil.PFS.Client
{
    [DataContract]
    internal class EphemeralCardRequest
    {
        [DataMember(Name = "content_snapshot")]
        public byte[] ContentSnapshot { get; internal set; }

        [DataMember(Name = "meta")]
        public EphemeralCardRequestMetaModel Meta { get; internal set; }
    }
}
