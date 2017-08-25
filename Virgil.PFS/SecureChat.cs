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
        private readonly SecureSessionHelper sessionHelper;
        private readonly SecureChatKeyHelper keyHelper;
        private readonly DateTime sessionExpireTime;

        public SecureChat(SecureChatParams parameters)
        {
            this.parameters = parameters;
            this.crypto = parameters.Crypto;
            this.myPrivateKey = parameters.IdentityPrivateKey;
            this.myIdentityCard = parameters.IdentityCard;
            this.keyHelper = new SecureChatKeyHelper(crypto, this.myIdentityCard.Id, parameters.LtPrivateKeyLifeDays);
            this.cardManager = new EphemeralCardManager(this.crypto, this.keyHelper, parameters.ServiceInfo);
            this.sessionHelper = new SecureSessionHelper(this.myIdentityCard.Id);
            this.sessionExpireTime = DateTime.Now.AddDays(this.parameters.SessionLifeDays);
        }

        public async Task RotateKeysAsync(int desireNumberOfCards = 10)
        {
            this.Cleanup();
            var numberOfOtCard = await this.cardManager.GetOtCardsCount(this.myIdentityCard.Id);
            if (desireNumberOfCards > numberOfOtCard.Active)
            {
                await cardManager.BootstrapCardsSet(
                    this.myIdentityCard,
                    this.myPrivateKey,
                    (desireNumberOfCards - numberOfOtCard.Active)
                    );

            }
        }

        private async void Cleanup()
        {
            var sessionInfos = this.sessionHelper.GetAllSessionStates();
            foreach (var sessionInfo in sessionInfos)
            {
                if (sessionInfo.SessionState.IsSessionExpired())
                {
                    this.CleanSessionDataByCardId(sessionInfo.CardId, sessionInfo.SessionState);
                }
            }
            var deletedLtKeyCardIds = this.keyHelper.LtKeyHolder().RemoveExpiredKeys();
           
            await this.RemoveExhaustedOtKeys();
        }

        private void RemoveSessionKeyBySessionId(byte[] sessionId)
        {
            var sessionIdBase64 = Convert.ToBase64String(sessionId);
            if (this.keyHelper.SessionKeyHolder()
                .IsKeyExist(sessionIdBase64))
            {
                this.keyHelper.SessionKeyHolder().RemoveKey(sessionIdBase64);
            }
        }

        private async Task RemoveExhaustedOtKeys()
        {
            //remove exhausted otcards, which don't belong to any session states more than 1 day
            var otKeys = this.keyHelper.OtKeyHolder().AllKeys();

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


        public async Task<ISession> StartNewSessionWithAsync(CardModel recipientCard, byte[] additionalData = null)
        {
            this.CheckExistingSession(recipientCard);

            var credentials = await this.cardManager.GetCredentialsByIdentityCard(recipientCard);
            var ephemeralKeyPair = crypto.GenerateKeys();

            var secureSession = new SecureSessionInitiator(this.crypto,
                this.myPrivateKey,
                this.myIdentityCard,
                ephemeralKeyPair.PrivateKey,
                recipientCard.Id,
                credentials,
                recipientCard,
                this.keyHelper,
                this.sessionHelper,
                additionalData,
                this.sessionExpireTime
                );
            return secureSession;
        }


        private void CheckExistingSession(CardModel recipientCard)
        {
            if (this.sessionHelper.ExistSessionState(recipientCard.Id))
            {
                var sessionState = this.sessionHelper.GetSessionState(recipientCard.Id);
                if (sessionState != null)
                {
                    if (sessionState.IsSessionExpired())
                    {
                        this.CleanSessionDataByCardId(recipientCard.Id, sessionState);
                    }
                    else
                    {
                        throw new SecureSessionException(
                            "Exist session for given recipient. Try to loadUpSession");
                    }
                }
            }
        }


        public ISession ActiveSession(string recipientCardId)
        {
            try
            {
                var sessionState = this.sessionHelper.GetSessionState(recipientCardId);
                if (sessionState.IsSessionExpired())
                {
                    this.CleanSessionDataByCardId(recipientCardId, sessionState);
                    return null;
                }
                return this.RecoverSession(sessionState);
            }
            catch (Exception)
            {
                return null;
            }

        }

        public void RemoveSession(string recipientCardId)
        {
            try
            {
                if (this.sessionHelper.ExistSessionState(recipientCardId))
                {
                    CleanSessionDataByCardId(
                        recipientCardId, 
                        this.sessionHelper.GetSessionState(recipientCardId)
                        );
                }
            }
            catch (Exception)
            {
                throw new SecureSessionHolderException("Remove session exception.");
            }
        }

        private void CleanSessionDataByCardId(string recipientCardId, SessionState sessionState)
        {
            this.RemoveSessionKeyBySessionId(sessionState.SessionId);
            this.sessionHelper.DeleteSessionState(recipientCardId);
        }


        public async Task<ISession> LoadUpSession(CardModel recipientCard, string msg, byte[] additionalData = null)
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

                var sessionResponder = new SecureSessionResponder(
                    this.crypto,
                    this.myPrivateKey,
                    this.myIdentityCard,
                    recipientCard,
                    additionalData,
                    this.keyHelper,
                    this.sessionHelper,
                    this.sessionExpireTime);
                sessionResponder.Decrypt(initialMessage);

                return sessionResponder;
            }
            else
            {
                return this.TryToRecoverSessionByMessage(recipientCard.Id, msg);
            }
        }

        private ISession TryToRecoverSessionByMessage(string recipientCardId, string msg)
        {
            var message = MessageHelper.ExtractMessage(msg);
            var sessionId = message.SessionId;
            var sessionState = this.sessionHelper.GetSessionState(recipientCardId);
            if (sessionState == null)
            {
                return null;
            }
            if (!Enumerable.SequenceEqual(sessionId, sessionState.SessionId))
            {
                throw new Exception("Session isn't found.");
            }

            return this.RecoverSession(sessionState);

        }

        private ISession RecoverSession(SessionState sessionState)
        {
            try
            {
               var sessionKey = this.keyHelper.SessionKeyHolder().LoadKeyByName(
                   sessionState.GetSessionIdBase64());
               return new CoreSession(sessionState.SessionId,
                    sessionKey.EncryptionKey, sessionKey.DecryptionKey, sessionState.AdditionalData);
            }
            catch (Exception)
            {
                throw new SecureSessionHolderException("Unknown session state");
            }
        }
        

        public void GentleReset()
        {
            foreach (var initiator in this.sessionHelper.GetAllSessionStates())
            {
                this.RemoveSession(initiator.CardId);
            }
            this.keyHelper.OtKeyHolder().RemoveAllKeys();
            this.keyHelper.LtKeyHolder().RemoveAllKeys();
        }
    }
}
