using System;
using Virgil.SDK.Client;
using Virgil.SDK.Cryptography;

namespace Virgil.PFS
{
    public class SecureChatParams
    {
        private const int SessionToBeAliveDays = 5;
        private const int LongTermPrivateKeyToBeAliveDays = 10;
        public ICrypto Crypto { get; protected set; }
        public ServiceInfo ServiceInfo { get; protected set; }
        public CardModel IdentityCard { get; protected set; }
        public IPrivateKey IdentityPrivateKey { get; protected set; }
        public int SessionLifeDays { get; protected set; }
        public int LtPrivateKeyLifeDays { get; protected set; }

        public SecureChatParams(ICrypto crypto,
            CardModel identityCard,
            IPrivateKey identityPrivateKey,
            ServiceInfo serviceInfo,
            int sessionToBeAliveDays = SessionToBeAliveDays,
            int longTermPrivateKeyToBeAliveDays = LongTermPrivateKeyToBeAliveDays
            )
        {
            this.Crypto = crypto;
            this.IdentityPrivateKey = identityPrivateKey;
            this.IdentityCard = identityCard;
            this.ServiceInfo = serviceInfo;
            if (longTermPrivateKeyToBeAliveDays < sessionToBeAliveDays)
            {
                throw new Exception("Sorry! Long term private key can't live less than session.");
            }
            if (sessionToBeAliveDays < 1)
            {
                throw new Exception("Very short session's lifetime.");
            }
            this.LtPrivateKeyLifeDays = longTermPrivateKeyToBeAliveDays;
            this.SessionLifeDays = sessionToBeAliveDays;
        }

        public SecureChatParams(ICrypto crypto,
            CardModel identityCard,
            IPrivateKey identityPrivateKey,
            string accessToken, 
            int sessionToBeAliveDays = SessionToBeAliveDays, 
            int longTermPrivateKeyToBeAliveDays = LongTermPrivateKeyToBeAliveDays) : this(
                crypto,
                identityCard,
                identityPrivateKey,
                new ServiceInfo()
                {
                    AccessToken = accessToken,
                    Address = "https://pfs.virgilsecurity.com"
                },
            sessionToBeAliveDays,
            longTermPrivateKeyToBeAliveDays
            )
        {
 
        }
    }
}