using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Virgil.PFS.Client;
using Virgil.PFS.Exceptions;
using Virgil.PFS.KeyUtils;
using Virgil.PFS.Session;
using Virgil.SDK.Client;
using Virgil.SDK.Common;
using Virgil.SDK.Cryptography;
using Virgil.SDK.Exceptions;

namespace Virgil.PFS
{
    public class SecureChat
    {
        private readonly SecureChatParams parameters;
        private readonly ICrypto crypto;
        private readonly IPrivateKey myPrivateKey;
        private readonly CardModel myIdentityCard;
        private readonly EphemeralCardManager cardManager;
        private readonly SecureChatKeyHelper keyHelper;
        private readonly DateTime sessionExpireTime;
        private readonly SessionManager sessionManager;

        public SecureChat(SecureChatParams parameters)
        {
            this.crypto = parameters.Crypto;
            this.myPrivateKey = parameters.IdentityPrivateKey;
            this.myIdentityCard = parameters.IdentityCard;
            this.keyHelper = new SecureChatKeyHelper(crypto, this.myIdentityCard.Id, parameters.LtPrivateKeyLifeDays);
            this.cardManager = new EphemeralCardManager(this.crypto, this.keyHelper, parameters.ServiceInfo);
            var sessionHelper = new SecureSessionHelper(this.myIdentityCard.Id, parameters.SessionStorage);
            this.sessionManager = new SessionManager(myIdentityCard, myPrivateKey, 
                crypto, sessionHelper, keyHelper, parameters.SessionLifeDays);
        }

        public async Task RotateKeysAsync(int desireNumberOfCards = 10)
        {
            await this.Cleanup();
            var numberOfOtCard = await this.cardManager.GetOtCardsCount(this.myIdentityCard.Id);
            var missingCards = ((desireNumberOfCards - numberOfOtCard.Active) > 0)
                ? (desireNumberOfCards - numberOfOtCard.Active) : 0;
            await cardManager.BootstrapCardsSet(
                this.myIdentityCard,
                this.myPrivateKey,
                missingCards
                );
        }

        private async Task Cleanup()
        {
            this.sessionManager.RemoveExpiredSessions();
            this.keyHelper.LtKeyHolder().RemoveExpiredKeys();

            await this.RemoveExhaustedOtKeys();
        }



        private async Task RemoveExhaustedOtKeys()
        {
            //remove exhausted otcards, which don't belong to any session states more than 1 day
            var otKeys = this.keyHelper.OtKeyHolder().AllKeys();
            if (otKeys.Count > 0)
            {
                var exhaustedOtCardIds = await this.cardManager.ValidateOtCards(this.myIdentityCard.Id, otKeys.Keys);
                var otKeysToBeRemoved = otKeys.Where(x => exhaustedOtCardIds.Contains(x.Key))
                    .ToDictionary(p => p.Key, p => p.Value);
                foreach (var otKey in otKeysToBeRemoved)
                {
                    if (otKey.Value.ExpiredAt == null)
                    {
                        this.keyHelper.OtKeyHolder().SetUpExpiredAt(otKey.Key);
                    }
                    else
                    {
                        if (otKey.Value.ExpiredAt <= DateTime.Now)
                        {
                            this.keyHelper.OtKeyHolder().RemoveKey(otKey.Key);
                        }
                    }
                }
            }
        }


        public async Task<CoreSession> StartNewSessionWithAsync(CardModel recipientCard, byte[] additionalData = null)
        {
            this.sessionManager.CheckExistingSession(recipientCard.Id);
            var credentials = await this.cardManager.GetCredentialsByIdentityCard(recipientCard);
            
            return this.sessionManager.InitializeInitiatorSession(recipientCard, credentials, additionalData);
        }


        public CoreSession ActiveSession(string recipientCardId)
        {
            return this.sessionManager.GetActiveSession(recipientCardId);
        }

        public void RemoveSession(string recipientCardId)
        {
            this.sessionManager.RemoveSession(recipientCardId);
        }

        public async Task<CoreSession> LoadUpSession(CardModel recipientCard, string msg, byte[] additionalData = null)
        {
            if (MessageHelper.IsInitialMessage(msg))
            {
                var initialMessage = MessageHelper.ExtractInitialMessage(msg);

                // Added new one time card
                try
                {
                    await this.cardManager.BootstrapCardsSet(this.myIdentityCard, this.myPrivateKey, 1);
                }
                catch (Exception)
                {
                    return null;
                }

                return this.sessionManager.InitializeResponderSession(recipientCard, initialMessage, additionalData);
            }
            else
            {
                return this.TryToRecoverSessionByMessage(recipientCard.Id, msg);
            }
        }

        private CoreSession TryToRecoverSessionByMessage(string recipientCardId, string msg)
        {
            var message = MessageHelper.ExtractMessage(msg);
            return this.sessionManager.LoadUpSession(message.SessionId, recipientCardId);
        }

        public void GentleReset()
        {
            this.sessionManager.RemoveAllSessions();
            this.keyHelper.OtKeyHolder().RemoveAllKeys();
            this.keyHelper.LtKeyHolder().RemoveAllKeys();
        } 
    }
}
