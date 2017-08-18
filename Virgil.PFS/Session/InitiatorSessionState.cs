using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Virgil.SDK.Client;

namespace Virgil.PFS
{
    public class InitiatorSessionState : SessionState
    {
        [JsonProperty("my_eph_key_name")]
        public string MyEphKeyName { get; protected set; }

        [JsonProperty("recipient_card")]
        public CardModel RecipientCard { get; protected set; }


        [JsonProperty("recipient_lt_card")]
        public CardModel RecipientLtCard { get; protected set; }


        [JsonProperty("recipient_ot_card")]
        public CardModel RecipientOtCard { get; protected set; }


        public InitiatorSessionState(byte[] sessionId,
            DateTime createdAt,
            DateTime expiredAt,
            byte[] additionalData,
            string myEphKeyName,
            CardModel recipientCard,
            CardModel recipientLtCard,
            CardModel recipientOtCard
            ) :
            base(sessionId, createdAt, expiredAt, additionalData)
        {
            this.MyEphKeyName = myEphKeyName;
            this.RecipientCard = recipientCard;
            this.RecipientLtCard = recipientLtCard;
            this.RecipientOtCard = recipientOtCard;
        }
    }
}
