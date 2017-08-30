using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Virgil.SDK.Client;

namespace Virgil.PFS
{
    internal class CredentialsModel
    {
        [JsonProperty("long_time_card")]
        public CardModel LTCard { get; set; }

        [JsonProperty("one_time_card")]
        public CardModel OTCard { get; set; }
}
}


