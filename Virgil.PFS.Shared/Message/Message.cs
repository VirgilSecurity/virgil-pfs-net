using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Virgil.PFS
{
    public class Message {
        [JsonProperty("session_id")]
        public byte[] SessionId { get; set; }

        [JsonProperty("salt")]
        public byte[] Salt { get; set; }

        [JsonProperty("ciphertext")]
        public byte[] CipherText { get; set; }
    }
}
