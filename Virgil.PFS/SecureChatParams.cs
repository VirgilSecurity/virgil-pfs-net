using Virgil.SDK.Client;
using Virgil.SDK.Cryptography;

namespace Virgil.PFS
{
    public class SecureChatParams
    {

        public ICrypto Crypto { get; protected set; }
        public ServiceInfo ServiceInfo { get; protected set; }
        public CardModel IdentityCard { get; protected set; }
        public IPrivateKey IdentityPrivateKey { get; protected set; }

        public SecureChatParams(ICrypto crypto, 
            CardModel identityCard, 
            IPrivateKey identityPrivateKey, 
            ServiceInfo serviceInfo
            )
        {
            this.Crypto = crypto;
            this.IdentityPrivateKey = identityPrivateKey;
            this.IdentityCard = identityCard;
            this.ServiceInfo = serviceInfo;
        }

        public SecureChatParams(ICrypto crypto, 
            CardModel identityCard, 
            IPrivateKey identityPrivateKey, 
            string accessToken)
        {
            this.Crypto = crypto;
            this.IdentityPrivateKey = identityPrivateKey;
            this.IdentityCard = identityCard;
            this.ServiceInfo = new ServiceInfo()
            {
                AccessToken = accessToken,
                Address = "https://pfs.virgilsecurity.com"
            };
        }
    }
}