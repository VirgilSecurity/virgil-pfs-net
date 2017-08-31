using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Virgil.PFS
{
    [DataContract]
    internal class InitialMessage
    {
        [JsonProperty("initiator_ic_id")]
        public String InitiatorIcId;

        [JsonProperty("responder_ic_id")]
        public String ResponderIcId;

        [JsonProperty("responder_ltc_id")]
        public String ResponderLtcId;

        [JsonProperty("responder_otc_id")]
        public String ResponderOtcId;

        [JsonProperty("eph")]
        public byte[] EphPublicKey;

        [JsonProperty("sign")]
        public byte[] EphPublicKeySignature;

        [JsonProperty("salt")]
        public byte[] Salt;

        [JsonProperty("ciphertext")]
        public byte[] CipherText;
    }
}
