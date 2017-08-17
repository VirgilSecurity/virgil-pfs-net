using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Virgil.PFS.Client;
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

        public SecureChat(SecureChatParams parameters)
        {
            this.parameters = parameters;
            this.crypto = parameters.Crypto;
            this.myPrivateKey = parameters.IdentityPrivateKey;
            this.myIdentityCard = parameters.IdentityCard;
            this.keyHelper = new SecureChatKeyHelper(crypto, this.myIdentityCard.Id);
            this.cardManager = new EphemeralCardManager(this.crypto, this.keyHelper, parameters.ServiceInfo);
            this.sessionHelper = new SecureSessionHelper(this.myIdentityCard.Id);
        }

        public async Task InitializeAsync(int desireNumberOfCards = 10)
        {
            this.Cleanup();
            await cardManager.BootstrapCardsSet(this.myIdentityCard, this.myPrivateKey);
        }

        private void Cleanup()
        {
            var sessionStates = this.sessionHelper.GetAllSessionStates();
            foreach (var initiator in sessionStates.Initiators)
            {
                var cardId = initiator.CardId;

                if (initiator.SessionState.IsSessionExpired())
                {
                    if (this.keyHelper.SessionKeyHolder().IsKeyExist(cardId))
                    {
                        this.keyHelper.SessionKeyHolder().RemoveKey(cardId);
                    }
                    this.sessionHelper.DeleteSessionState(cardId);
                }
            }

            foreach (var responder in sessionStates.Responders)
            {
                var cardId = responder.CardId;

                if (responder.SessionState.IsSessionExpired())
                {
                    /*
                     * todo remove ltkey
                    if (this.keyHelper.LtKeyHolder().IsKeyExist(cardId))
                    {
                        this.keyHelper.LtKeyHolder().RemoveKey(cardId);
                    }*/

                    if (this.keyHelper.OtKeyHolder().IsKeyExist(cardId))
                    {
                        this.keyHelper.OtKeyHolder().RemoveKey(cardId);
                    }

                    this.sessionHelper.DeleteSessionState(cardId);
                }
            }
            //todo remove otcards, which don't belong to any session states
            //this.cardManager.ValidateOtCards(this.myIdentityCard.Id, new string[] {"cardId1", "cardId2"});
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
                additionalData
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
                        throw new Exception("Exist session for given recipient. Try to loadUpSession");
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
                if (this.keyHelper.SessionKeyHolder().IsKeyExist(recipientCardId))
                {
                    this.keyHelper.SessionKeyHolder().RemoveKey(recipientCardId);
                }

                var sessionState = this.sessionHelper.GetSessionState(recipientCardId);

                if (sessionState != null)
                {
                    if (sessionState.GetType() == typeof(ResponderSessionState))
                    {
                        var responderOtCardId = ((ResponderSessionState)sessionState).ResponderOtCardId;
                        if (responderOtCardId != null)
                        {
                            this.keyHelper.OtKeyHolder().RemoveKey(responderOtCardId);
                        }
                    }
                    this.sessionHelper.DeleteSessionState(recipientCardId);

                }

            }
            catch (Exception)
            {
                //todo virgil exception
                throw new Exception("Remove session exception.");
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
                    this.keyHelper);
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
                    throw new Exception("Unknown session state");
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
                true);
            return sessionResponder;
        }



        public void GentleReset()
        {
            var sessionStateIds = this.sessionHelper.GetAllSessionStateIds();

            foreach (var sessionStateId in sessionStateIds)
            {
                if (this.keyHelper.SessionKeyHolder().IsKeyExist(sessionStateId))
                {
                    this.keyHelper.SessionKeyHolder().RemoveKey(sessionStateId);
                }
            }

            var responderSessionStates = this.sessionHelper.GetAllResponderSessionStates();
            foreach (var responderSessionState in responderSessionStates)
            {
                if (responderSessionState.ResponderOtCardId != null &&
                    this.keyHelper.OtKeyHolder().IsKeyExist(responderSessionState.ResponderOtCardId))
                {
                    this.keyHelper.OtKeyHolder().RemoveKey(responderSessionState.ResponderOtCardId);
                }
            }

            this.sessionHelper.DeleteAllSessionStates();
        }
    }
}
