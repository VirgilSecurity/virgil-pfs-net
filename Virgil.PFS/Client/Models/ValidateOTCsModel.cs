using Newtonsoft.Json;

namespace Virgil.PFS
{
    internal class ValidateOtcsModel
    {
        [JsonProperty("exhausted_one_time_cards_ids")]
        public string[] ExhaustedOtCardsIds { get; set; }
    }
}