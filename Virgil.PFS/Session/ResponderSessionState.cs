using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Virgil.SDK.Client;

namespace Virgil.PFS
{
    public class ResponderSessionState : SessionState
    {
        /* [JsonProperty("initiator_eph_public_key_data")]
         public byte[] InitiatorEphPublicKeyData { get; protected set; }

         [JsonProperty("initiator_identity_card_id")]
         public string InitiatorIdentityCardId { get; protected set; }

         [JsonProperty("initiator_identity_public_key")]
         public byte[] InitiatorIdentityPublicKey { get; protected set; }

         [JsonProperty("responder_lt_card_id")]
         public string ResponderLtCardId { get; protected set; }

         [JsonProperty("responder_ot_card_id")]
         public string ResponderOtCardId { get; protected set; }

         public ResponderSessionState(byte[] sessionId,
             DateTime createdAt, 
             DateTime expiredAt,
             byte[] additionalData,
             byte[] initiatorEphPublicKeyData,
             string initiatorIdentityCardId,
             byte[] initiatorIdentityPublicKey,
             string responderLtCardId,
             string responderOtCardId
             ) : base(sessionId, createdAt, expiredAt, additionalData)
         {
             this.InitiatorEphPublicKeyData = initiatorEphPublicKeyData;
             this.InitiatorIdentityCardId = initiatorIdentityCardId;
             this.InitiatorIdentityPublicKey = initiatorIdentityPublicKey;
             this.ResponderLtCardId = responderLtCardId;
             this.ResponderOtCardId = responderOtCardId;
         }*/

        [JsonProperty("initiator_eph_public_key_data")]
        public byte[] InitiatorEphPublicKeyData { get; protected set; }

        [JsonProperty("initiator_identity_card")]
        public CardModel InitiatorIdentityCard { get; protected set; }

        [JsonProperty("responder_lt_card-id")]
        public string ResponderLtCardId { get; protected set; }

        [JsonProperty("responder_ot_card_id")]
        public string ResponderOtCardId { get; protected set; }

        public ResponderSessionState(byte[] sessionId,
            DateTime createdAt,
            DateTime expiredAt,
            byte[] additionalData,
            byte[] initiatorEphPublicKeyData,
            CardModel initiatorIdentityCard,
            string responderLtCardId,
            string responderOtCardId
        ) : base(sessionId, createdAt, expiredAt, additionalData)
        {
            this.InitiatorEphPublicKeyData = initiatorEphPublicKeyData;
            this.InitiatorIdentityCard = initiatorIdentityCard;
            this.ResponderLtCardId = responderLtCardId;
            this.ResponderOtCardId = responderOtCardId;
        }
    }
}
