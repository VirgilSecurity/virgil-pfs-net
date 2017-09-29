using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Virgil.Crypto.Pfs;
using Virgil.PFS.Exceptions;
using Virgil.SDK.Client;
using Virgil.SDK.Cryptography;

namespace Virgil.PFS.Session
{
    internal class SessionManager
    {
        private readonly SessionStorageManager sessionStorageManager;
        private readonly KeyStorageManger keyStorageManger;
        private readonly ICrypto crypto;
        private readonly IPrivateKey identityPrivateKey;
        private readonly int sessionLifeDays;
        private readonly SessionInitializer sessionInitializer;
        public SessionManager(CardModel identityCard,
            IPrivateKey identityPrivateKey, ICrypto crypto,
            SessionStorageManager sessionStorageManager, KeyStorageManger keyStorageManger, int sessionLifeDays)
        {
            this.identityPrivateKey = identityPrivateKey;
            this.crypto = crypto;
            this.sessionStorageManager = sessionStorageManager;
            this.keyStorageManger = keyStorageManger;
            this.sessionLifeDays = sessionLifeDays;
            this.sessionInitializer = new SessionInitializer(crypto, identityPrivateKey, identityCard);
        }
        public SecureSession GetActiveSession(string recipientCardId)
        {
            try
            {
                var sessionState = this.sessionStorageManager.GetNewestSessionState(recipientCardId);
                if (!sessionState.IsSessionExpired())
                {
                    return this.RecoverSession(recipientCardId, sessionState);
                }
            }
            catch (Exception)
            {
            }
            return null;
        }

        public SecureSession InitializeInitiatorSession(CardModel recipientCard,
            CredentialsModel credentials, byte[] additionalData = null)
        {
            var expiredAt = DateTime.Now.AddDays(this.sessionLifeDays);
            var session = this.sessionInitializer.InitializeInitiatorSession(
                recipientCard, credentials, additionalData, expiredAt);

            this.SaveSession(session, recipientCard.Id);
            return session;
        }

        public SecureSession InitializeResponderSession(CardModel initiatorCard, InitialMessage message,
            byte[] additionalData)
        {
            this.ValidateInitiatorEphPublicKey(initiatorCard, message);
            this.ValidateInitiatorIdentityCardId(initiatorCard.Id, message);

            var expiredAt = DateTime.Now.AddDays(this.sessionLifeDays);

            var myPrivateKeyData = this.crypto.ExportPrivateKey(this.identityPrivateKey);
            var myLtPrivateKey = this.crypto.ExportPrivateKey(
                this.keyStorageManger.LtKeyStorage().LoadKeyByName(message.ResponderLtcId));

            byte[] myOtPrivateKeyData = null;
            if (message.ResponderOtcId != null)
            {
                myOtPrivateKeyData = this.crypto.ExportPrivateKey(
                    this.keyStorageManger.OtKeyStorage().LoadKeyByName(message.ResponderOtcId));
            }
            var session = sessionInitializer.InitializeResponderSession(initiatorCard.SnapshotModel.PublicKeyData,
                message.EphPublicKey,
                additionalData, myLtPrivateKey,
                myOtPrivateKeyData, myPrivateKeyData, expiredAt);

            if (this.keyStorageManger.OtKeyStorage().IsKeyExist(message.ResponderOtcId))
            {
                this.keyStorageManger.OtKeyStorage().RemoveKey(message.ResponderOtcId);
            }
            this.SaveSession(session, initiatorCard.Id);

            return session;
        }

        private void ValidateInitiatorIdentityCardId(string initiatorCardId, InitialMessage message)
        {
            if (message.InitiatorIcId != initiatorCardId)
            {
                throw new SecureSessionException(
                    "Initiator identity card id for this session and InitiationMessage doesn't match.");
            }
        }


        private void ValidateInitiatorEphPublicKey(CardModel initiatorCard, InitialMessage message)
        {
            var initiatorPublicKey =
                this.crypto.ImportPublicKey(initiatorCard.SnapshotModel.PublicKeyData);
            if (!this.crypto.Verify(message.EphPublicKey, message.EphPublicKeySignature, initiatorPublicKey))
            {
                throw new SecureSessionException("Error validating initiator signature.");
            }
        }

        public void SaveSession(SecureSession session, string recipientCardId)
        {
            this.keyStorageManger.SessionKeyStorage().SaveKeyByName(session.GetKey(), recipientCardId);

            var sessionState = new SessionState(
                session.GetId(),
                session.CreatedAt(),
                session.ExpiredAt(),
                session.GetAdditionalData());
            this.sessionStorageManager.SaveSessionState(sessionState, recipientCardId);
        }

        public void RemoveSession(string recipientCardId, string sessionId)
        {
            try
            {
                if (this.sessionStorageManager.ExistSessionState(recipientCardId, sessionId))
                {
                    CleanSessionDataByCardId(recipientCardId);
                }
            }
            catch (Exception)
            {
                throw new SessionStorageException("Remove session exception.");
            }
        }

        public void RemoveSessions(string recipientCardId)
        {
            try
            {
                if (this.sessionStorageManager.ExistSessionStates(recipientCardId))
                {
                    this.sessionStorageManager.DeleteSessionStates(recipientCardId);
                    //todo delete session keys
                }
            }
            catch (Exception)
            {
                throw new SessionStorageException("Remove session exception.");
            }
        }


        private void CleanSessionDataByCardId(string recipientCardId)
        {
            this.RemoveSessionKey(recipientCardId);
            this.sessionStorageManager.DeleteSessionStates(recipientCardId);
        }

        private void RemoveSessionKey(string cardId)
        {
            if (this.keyStorageManger.SessionKeyStorage()
                .IsKeyExist(cardId))
            {
                this.keyStorageManger.SessionKeyStorage().RemoveKey(cardId);
            }
        }

        public void RemoveExpiredSessions()
        {
            var sessionInfos = this.sessionStorageManager.GetAllSessionStates();
            foreach (var sessionInfo in sessionInfos)
            {
                // todo in the next version to check as isShouldBeDeleted
                if (sessionInfo.SessionState.IsShouldBeDeleted())
                {
                    this.CleanSessionDataByCardId(sessionInfo.CardId);
                }
            }
        }

        public void CheckExistingSessionOnStart(string recipientCardId)
        {
            if (this.sessionStorageManager.ExistSessionStates(recipientCardId))
            {
                var sessionState = this.sessionStorageManager.GetNewestSessionState(recipientCardId);
                if (sessionState != null && !sessionState.IsSessionExpired())
                {
                    throw new SecureSessionException(
                        "Exist session for given recipient. Try to ActiveSession.");
                }
            }
        }

        public void RemoveAllSessions()
        {
            this.sessionStorageManager.RemoveAllSessionStates();
            // todo remove all session keys 
        }

        
        public SecureSession LoadUpSession(byte[] sessionId, string recipientCardId)
        {
            var sessionState = this.sessionStorageManager.GetSessionStates(recipientCardId)
                .First(el => Enumerable.SequenceEqual(sessionId, el.SessionId));
            if (sessionState == null)
            {
                throw new SessionStorageException("Session isn't found.");
            }
            return this.RecoverSession(recipientCardId, sessionState);
        }


        private SecureSession RecoverSession(string recipientCardId, SessionState sessionState)
        {
            try
            {
                var sessionKey = this.keyStorageManger.SessionKeyStorage().LoadKeyByName(
                    recipientCardId);
                return new SecureSession(sessionState.SessionId,
                     sessionKey.EncryptionKey,
                     sessionKey.DecryptionKey,
                     sessionState.AdditionalData,
                     sessionState.CreatedAt,
                     sessionState.ExpiredAt);
            }
            catch (Exception)
            {
                throw new SessionStorageException("Unknown session state");
            }
        }

    }
}
