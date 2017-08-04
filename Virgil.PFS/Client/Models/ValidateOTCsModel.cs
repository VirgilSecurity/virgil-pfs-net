using Newtonsoft.Json;

namespace Virgil.PFS
{
    public class ValidateOtcsModel
    {
        [JsonProperty("exhausted_one_time_cards_ids")]
        public string[] ExhaustedOtCardsIds { get; set; }
    }
}