using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Virgil.PFS.Client;
using Virgil.PFS.Exceptions;
using Virgil.PFS.KeyUtils;
using Virgil.PFS.Session;
using Virgil.PFS.Shared.KeyUtils;
using Virgil.SDK.Client;
using Virgil.SDK.Common;
using Virgil.SDK.Cryptography;
using Virgil.SDK.Exceptions;

namespace Virgil.PFS
{
    public class SecureChat
    {
        private readonly ICrypto crypto;
        private readonly IPrivateKey myPrivateKey;
        private readonly CardModel myIdentityCard;
        private readonly EphemeralCardManager cardManager;
        private readonly KeyStorageManger keyStorageManger;
        private readonly SessionManager sessionManager;
        private readonly KeysRotator keysRotator;

        public SecureChat(SecureChatPreferences parameters)
        {
            this.crypto = parameters.Crypto;
            this.myPrivateKey = parameters.IdentityPrivateKey;
            this.myIdentityCard = parameters.IdentityCard;
            this.keyStorageManger = new KeyStorageManger(crypto, this.myIdentityCard.Id, parameters.LtPrivateKeyLifeDays);
            this.cardManager = new EphemeralCardManager(this.crypto, this.keyStorageManger, parameters.ServiceInfo);
            var sessionStorage = new DefaultUserDataStorage(this.myIdentityCard.Id);
            var sessionHelper = new SessionStorageManager((IUserDataStorage)sessionStorage);
            this.sessionManager = new SessionManager(myIdentityCard, myPrivateKey, 
                crypto, sessionHelper, keyStorageManger, parameters.SessionLifeDays);
            this.keysRotator = new KeysRotator(this.myIdentityCard, this.myPrivateKey, this.cardManager);
        }

        public async Task RotateKeysAsync(int desireNumberOfCards = 10)
        {
           await this.Cleanup();
           await this.keysRotator.Rotate(desireNumberOfCards);
        }

        private async Task Cleanup()
        {
            this.sessionManager.RemoveExpiredSessions();
            this.keyStorageManger.LtKeyStorage().RemoveExpiredKeys();
            await this.cardManager.RemoveKeysForExhaustedOtCards(this.myIdentityCard.Id);
        }


        public async Task<SecureSession> StartNewSessionWithAsync(CardModel recipientCard, byte[] additionalData = null)
        {
            this.sessionManager.CheckExistingSessionOnStart(recipientCard.Id);
            var credentials = await this.cardManager.GetCredentialsByIdentityCard(recipientCard);
            
            return this.sessionManager.InitializeInitiatorSession(recipientCard, credentials, additionalData);
        }


        public SecureSession ActiveSession(string recipientCardId)
        {
            return this.sessionManager.GetActiveSession(recipientCardId);
        }

        public void RemoveSession(string recipientCardId, string sessionId)
        {
            this.sessionManager.RemoveSession(recipientCardId, sessionId);
        }

        public void RemoveSession(string recipientCardId)
        {
            this.sessionManager.RemoveSessions(recipientCardId);
        }

        public async Task<SecureSession> LoadUpSession(CardModel recipientCard, string msg, byte[] additionalData = null)
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

        private SecureSession TryToRecoverSessionByMessage(string recipientCardId, string msg)
        {
            var message = MessageHelper.ExtractMessage(msg);
            return this.sessionManager.LoadUpSession(message.SessionId, recipientCardId);
        }

        public void GentleReset()
        {
            this.sessionManager.RemoveAllSessions();
            this.keyStorageManger.RemoveAllOtLtKeys();
        } 
    }
}
