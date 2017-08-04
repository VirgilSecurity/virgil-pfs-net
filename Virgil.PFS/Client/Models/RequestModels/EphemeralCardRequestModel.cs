using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Virgil.PFS.Client.Models
{
    public class EphemeralCardRequestModel
    {
        [JsonProperty("content_snapshot")]
        public byte[] ContentSnapshot { get; set; }

        [JsonProperty("meta")]
        public EphemeralCardRequestMetaModel Meta { get; set; }
    }
}
