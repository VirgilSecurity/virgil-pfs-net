using System.Collections.Generic;
using Virgil.PFS.Client.Models;

namespace Virgil.PFS
{
    using Newtonsoft.Json;

    public class RecipientRequestModel
    {
        [JsonProperty("long_time_card")]
        public EphemeralCardRequestModel LTCardModel { get; set; }

        [JsonProperty("one_time_cards")]
        public List<EphemeralCardRequestModel> OTCardModels { get; set; }
    }
}