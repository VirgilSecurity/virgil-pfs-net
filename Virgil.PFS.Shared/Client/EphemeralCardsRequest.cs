using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;
using Virgil.PFS.Client.Models;

namespace Virgil.PFS
{
    [DataContract]
    internal class EphemeralCardsRequest
    {

        [DataMember(Name = "long_time_card")]
        public EphemeralCardRequestModel LtcRequestModel { get; set; }

        [DataMember(Name = "one_time_cards")]
        public List<EphemeralCardRequestModel> OtcRequestModels { get; set; }

        public EphemeralCardsRequest()
        {
            OtcRequestModels = new List<EphemeralCardRequestModel>();

        }
    }
}
