using System.Collections.Generic;
using Newtonsoft.Json;

namespace Virgil.PFS.Client.Models
{
    public class EphemeralCardRequestMetaModel
    {
        [JsonProperty("signs")]
        public Dictionary<string, byte[]> Signatures { get; set; }
    }
}