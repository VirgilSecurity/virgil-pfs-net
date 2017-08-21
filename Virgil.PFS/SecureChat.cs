using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Virgil.PFS.Client;
using Virgil.PFS.Exceptions;
using Virgil.PFS.KeyUtils;
using Virgil.SDK.Client;
using Virgil.SDK.Common;
using Virgil.SDK.Cryptography;

namespace Virgil.PFS
{
    public class SecureChat
    {
        private readonly SecureChatParams parameters;
        private readonly ICrypto crypto;
        private readonly IPrivateKey myPrivateKey;
        private readonly CardModel myIdentityCard;
        private readonly SecureSession session;
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

        public async Task InitializeAsync(int desireNumberOfCards = 10)
        {
            this.Cleanup();
            await cardManager.BootstrapCardsSet(this.myIdentityCard, this.myPrivateKey, desireNumberOfCards);
        }

        private async void Cleanup()
        {
            var sessionStates = this.sessionHelper.GetAllSessionStates();
            foreach (var initiator in sessionStates.Initiators)
            {
                if (initiator.SessionState.IsSessionExpired())
                {
                    this.RemoveInitiatorSession(initiator.CardId);
                }
            }
            //check ltcard expired
            //if expired - delete all related responder sessions
            // if isn't expired - check each related responder session
            var deletedLtKeyCardIds = this.keyHelper.LtKeyHolder().RemoveExpiredKeys();
            foreach (var responder in sessionStates.Responders)
            {
                if (deletedLtKeyCardIds.Contains(responder.SessionState.ResponderLtCardId) ||
                        responder.SessionState.IsSessionExpired())
                {
                    this.RemoveResponderSession(responder.CardId, responder.SessionState);
                }
            }

            await this.RemoveExhaustedOtKeys();
        }

        private void RemoveInitiatorSession(string cardId)
        {
            if (this.keyHelper.SessionKeyHolder().IsKeyExist(cardId))
            {
                this.keyHelper.SessionKeyHolder().RemoveKey(cardId);
            }
            this.sessionHelper.DeleteSessionState(cardId);

        }

        private void RemoveResponderSession(string cardId, ResponderSessionState sessionState)
        {
            var otCardId = sessionState.ResponderOtCardId;

            if (this.keyHelper.OtKeyHolder().IsKeyExist(otCardId))
            {
                this.keyHelper.OtKeyHolder().RemoveKey(otCardId);
            }
            this.sessionHelper.DeleteSessionState(cardId);
        }

        private async Task RemoveExhaustedOtKeys()
        {
            //remove exhausted otcards, which don't belong to any session states
            var otKeys = this.keyHelper.OtKeyHolder().AllKeys();

            var exhaustedOtCardIds = await this.cardManager.ValidateOtCards(this.myIdentityCard.Id, otKeys.Keys);
            var otKeysToBeRemoved = otKeys.Where(x => exhaustedOtCardIds.Contains(x.Key))
                .ToDictionary(p => p.Key, p => p.Value);
            foreach (var otKey in otKeysToBeRemoved)
            {
                this.keyHelper.OtKeyHolder().RemoveKey(otKey.Key);
            }
        }


        public async Task<SecureSession> StartNewSessionWithAsync(CardModel recipientCard, byte[] additionalData = null)
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
                        this.RemoveSession(recipientCard.Id);
                    }
                    else
                    {
                        throw new SecureSessionException(
                            "Exist session for given recipient. Try to loadUpSession");
                    }

                }
            }
        }


        public SecureSession ActiveSession(string recipientCardId)
        {
            try
            {
                var sessionState = this.sessionHelper.GetSessionState(recipientCardId);
                if (sessionState.IsSessionExpired())
                {
                    this.RemoveSession(recipientCardId);
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
            // should remove:
            //  session ephemeral key - for session type 'initiator'
            //  otPrivateKey for session type 'responder'
            //  session state 
            try
            {
                if (this.sessionHelper.ExistSessionState(recipientCardId))
                {
                    var sessionState = this.sessionHelper.GetSessionState(recipientCardId);
                    if (sessionState.GetType() == typeof(ResponderSessionState))
                    {
                        this.RemoveResponderSession(recipientCardId, (ResponderSessionState)sessionState);
                    }
                    else
                    {
                        this.RemoveInitiatorSession(recipientCardId);
                    }
                }
            }
            catch (Exception)
            {
                throw new SecureSessionHolderException("Remove session exception.");
            }
        }


        public SecureSession LoadUpSession(CardModel recipientCard, string msg, byte[] additionalData = null)
        {
            if (MessageHelper.IsInitialMessage(msg))
            {
                var initialMessage = MessageHelper.ExtractInitialMessage(msg);

                var sessionResponder = new SecureSessionResponder(
                    this.crypto,
                    this.myPrivateKey,
                    this.myIdentityCard,
                    recipientCard,
                    additionalData,
                    this.keyHelper,
                    this.sessionExpireTime);
                sessionResponder.Decrypt(initialMessage);

                return sessionResponder;
            }
            else
            {
                return this.TryToRecoverSessionByMessage(recipientCard.Id, msg);
            }
        }

        private SecureSession TryToRecoverSessionByMessage(string recipientCardId, string msg)
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

        private SecureSession RecoverSession(SessionState sessionState)
        {
            if (sessionState.GetType() == typeof(InitiatorSessionState))
            {
                return this.RecoverSessionInitiator((InitiatorSessionState)sessionState);
            }
            else
            {
                if (sessionState.GetType() == typeof(ResponderSessionState))
                {
                    return this.RecoverSessionResponder((ResponderSessionState)sessionState);
                }
                else
                {
                    throw new SecureSessionHolderException("Unknown session state");
                }
            }
        }

        private SecureSessionInitiator RecoverSessionInitiator(InitiatorSessionState sessionState)
        {
            var myEphPrivateKey = this.keyHelper.SessionKeyHolder().LoadKeyByName(sessionState.MyEphKeyName);

            var secureSession = new SecureSessionInitiator(this.crypto,
                this.myPrivateKey,
                this.myIdentityCard,
                myEphPrivateKey,
                sessionState.MyEphKeyName,
                new CredentialsModel() { LTCard = sessionState.RecipientLtCard, OTCard = sessionState.RecipientOtCard },
                sessionState.RecipientCard,
                sessionState.AdditionalData,
                sessionState.ExpiredAt,
                true
            );
            return secureSession;

        }

        private SecureSessionResponder RecoverSessionResponder(ResponderSessionState sessionState)
        {
            var sessionResponder = new SecureSessionResponder(this.crypto,
                this.myPrivateKey,
                this.myIdentityCard,
                sessionState.InitiatorIdentityCard,
                sessionState.AdditionalData,
                this.keyHelper,
                sessionState.InitiatorEphPublicKeyData,
                sessionState.ResponderLtCardId,
                sessionState.ResponderOtCardId,
                sessionState.ExpiredAt);
            return sessionResponder;
        }


        public void GentleReset()
        {
            var sessionStates = this.sessionHelper.GetAllSessionStates();
            foreach (var initiator in sessionStates.Initiators)
            {
                this.RemoveInitiatorSession(initiator.CardId);
            }

            foreach (var responder in sessionStates.Responders)
            {
                this.RemoveResponderSession(responder.CardId, responder.SessionState);
            }

            this.keyHelper.OtKeyHolder().RemoveAllKeys();
            this.keyHelper.LtKeyHolder().RemoveAllKeys();
            this.keyHelper.SessionKeyHolder().RemoveAllKeys();
        }
    }
}
