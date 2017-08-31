﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Virgil.PFS.Session
{
    internal class SessionState
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
        // session should live one extra day after expiration
        public bool IsShouldBeDeleted()
        {
            return (DateTime.Now > this.ExpiredAt.AddDays(1));
        }

        public string GetSessionIdBase64()
        {
            return Convert.ToBase64String(this.SessionId);
        }

    }
}
