using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Virgil.PFS
{
    public class SessionState
    {
        [JsonProperty("additional_data")]
        public byte[] AdditionalData { get; protected set; }

        [JsonProperty("session_id")]
        public byte[] SessionId { get; protected set; }

        [JsonProperty("expired_at")]
        public DateTime ExpiredAt { get; protected set; }

        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; protected set; }

        public SessionState(byte[] sessionId, DateTime createdAt, DateTime expiredAt, byte[] additionalData)
        {
            this.CreatedAt = createdAt;
            this.ExpiredAt = expiredAt;
            this.SessionId = sessionId;
            this.AdditionalData = additionalData;
        }

        public bool IsSessionExpired()
        {
            return (DateTime.Now > this.ExpiredAt);
        }

    }
}
