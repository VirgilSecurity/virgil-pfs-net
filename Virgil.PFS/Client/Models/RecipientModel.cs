using System.Collections.Generic;
using Virgil.SDK.Client;

namespace Virgil.PFS
{
    using Newtonsoft.Json;

    public class RecipientModel
    {
        [JsonProperty("long_time_card")]
        public CardModel LTCardModel { get; set; }

        [JsonProperty("one_time_cards")]
        public List<CardModel> OTCardModels { get; set; }
    }
}