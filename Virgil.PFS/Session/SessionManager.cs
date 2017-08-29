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
    class SessionManager
    {
        private readonly SecureSessionHelper sessionHelper;
        private readonly SecureChatKeyHelper keyHelper;
        private readonly ICrypto crypto;
        private readonly IPrivateKey identityPrivateKey;
        private readonly CardModel identityCard;
        private readonly int sessionLifeDays;
        private readonly SessionInitializer sessionInitializer;
        public SessionManager(CardModel identityCard, 
            IPrivateKey identityPrivateKey, ICrypto crypto,
            SecureSessionHelper sessionHelper, SecureChatKeyHelper keyHelper, int sessionLifeDays)
        {
            this.identityCard = identityCard;
            this.identityPrivateKey = identityPrivateKey;
            this.crypto = crypto;
            this.sessionHelper = sessionHelper;
            this.keyHelper = keyHelper;
            this.sessionLifeDays = sessionLifeDays;
            this.sessionInitializer = new SessionInitializer(crypto, identityPrivateKey, identityCard);
        }
        public CoreSession GetActiveSession(string recipientCardId)
        {
            try
            {
                var sessionState = this.sessionHelper.GetSessionState(recipientCardId);
                if (sessionState.IsSessionExpired())
                {
                    this.CleanSessionDataByCardId(recipientCardId);
                    return null;
                }
                return this.RecoverSession(recipientCardId, sessionState);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public CoreSession InitializeInitiatorSession(CardModel recipientCard, 
            CredentialsModel credentials, byte[] additionalData = null)
        {
            var expiredAt = DateTime.Now.AddDays(this.sessionLifeDays);
            var session = this.sessionInitializer.InitializeInitiatorSession(
                recipientCard, credentials, additionalData, expiredAt);
          
            this.SaveSession(session, recipientCard.Id);
            return session;
        }

        public CoreSession InitializeResponderSession(CardModel initiatorCard, InitialMessage message,
            byte[] additionalData)
        {
            this.ValidateInitiatorEphPublicKey(initiatorCard, message);
            this.ValidateInitiatorIdentityCardId(initiatorCard.Id, message);

            var expiredAt = DateTime.Now.AddDays(this.sessionLifeDays);

            var myPrivateKeyData = this.crypto.ExportPrivateKey(this.identityPrivateKey);
            var myLtPrivateKey = this.crypto.ExportPrivateKey(
                this.keyHelper.LtKeyHolder().LoadKeyByName(message.ResponderLtcId));

            byte[] myOtPrivateKeyData = null;
            if (message.ResponderOtcId != null)
            {
                myOtPrivateKeyData = this.crypto.ExportPrivateKey(
                    this.keyHelper.OtKeyHolder().LoadKeyByName(message.ResponderOtcId));
            }
            var session = sessionInitializer.InitializeResponderSession(initiatorCard.SnapshotModel.PublicKeyData, 
                message.EphPublicKey, 
                additionalData, myLtPrivateKey, 
                myOtPrivateKeyData, myPrivateKeyData, expiredAt);

            if (this.keyHelper.OtKeyHolder().IsKeyExist(message.ResponderOtcId))
            {
                this.keyHelper.OtKeyHolder().RemoveKey(message.ResponderOtcId);
            }
            this.SaveSession(session, initiatorCard.Id);

            return session;
        }

        private void ValidateInitiatorIdentityCardId(string initiatorCardId, InitialMessage message)
        {
            if (message.InitiatorIcId != initiatorCardId)
            {
                throw new SecureSessionResponderException(
                    "Initiator identity card id for this session and InitiationMessage doesn't match.");
            }
        }


        private void ValidateInitiatorEphPublicKey(CardModel initiatorCard, InitialMessage message)
        {
            var initiatorPublicKey =
                this.crypto.ImportPublicKey(initiatorCard.SnapshotModel.PublicKeyData);
            if (!this.crypto.Verify(message.EphPublicKey, message.EphPublicKeySignature, initiatorPublicKey))
            {
                throw new SecureSessionResponderException("Error validating initiator signature.");
            }
        }

        public void SaveSession(CoreSession session, string recipientCardId)
        {
            this.keyHelper.SessionKeyHolder().SaveKeyByName(session.GetKey(), recipientCardId);

            var sessionState = new SessionState(
                session.GetId(),
                session.CreatedAt(),
                session.ExpiredAt(),
                session.GetAdditionalData());
            this.sessionHelper.SaveSessionState(sessionState, recipientCardId);
        }

        public void RemoveSession(string recipientCardId)
        {
            try
            {
                if (this.sessionHelper.ExistSessionState(recipientCardId))
                {
                    CleanSessionDataByCardId(recipientCardId);
                }
            }
            catch (Exception)
            {
                throw new SecureSessionHolderException("Remove session exception.");
            }
        }

        private void CleanSessionDataByCardId(string recipientCardId)
        {
            this.RemoveSessionKey(recipientCardId);
            this.sessionHelper.DeleteSessionState(recipientCardId);
        }

        private void RemoveSessionKey(string cardId)
        {
            if (this.keyHelper.SessionKeyHolder()
                .IsKeyExist(cardId))
            {
                this.keyHelper.SessionKeyHolder().RemoveKey(cardId);
            }
        }

        public void RemoveExpiredSessions()
        {
            var sessionInfos = this.sessionHelper.GetAllSessionStates();
            foreach (var sessionInfo in sessionInfos)
            {
                if (sessionInfo.SessionState.IsShouldBeDeleted())
                {
                    this.CleanSessionDataByCardId(sessionInfo.CardId);
                }
            }
        }

        public void CheckExistingSession(string recipientCardId)
        {
            if (this.sessionHelper.ExistSessionState(recipientCardId))
            {
                var sessionState = this.sessionHelper.GetSessionState(recipientCardId);
                if (sessionState != null)
                {
                    if (sessionState.IsSessionExpired())
                    {
                        this.CleanSessionDataByCardId(recipientCardId);
                    }
                    else
                    {
                        throw new SecureSessionException(
                            "Exist session for given recipient. Try to loadUpSession");
                    }
                }
            }
        }

        public void RemoveAllSessions()
        {
            foreach (var session in this.sessionHelper.GetAllSessionStates())
            {
                this.RemoveSession(session.CardId);
            }
        }

        public CoreSession LoadUpSession(byte[] sessionId, string recipientCardId)
        {
            var sessionState = this.sessionHelper.GetSessionState(recipientCardId);
            if (sessionState == null)
            {
                return null;
            }
            if (!Enumerable.SequenceEqual(sessionId, sessionState.SessionId))
            {
                throw new Exception("Session isn't found.");
            }

            return this.RecoverSession(recipientCardId, sessionState);
        }
        private CoreSession RecoverSession(string recipientCardId, SessionState sessionState)
        {
            try
            {
                var sessionKey = this.keyHelper.SessionKeyHolder().LoadKeyByName(
                    recipientCardId);
                return new CoreSession(sessionState.SessionId,
                     sessionKey.EncryptionKey, 
                     sessionKey.DecryptionKey, 
                     sessionState.AdditionalData,
                     sessionState.CreatedAt, 
                     sessionState.ExpiredAt);
            }
            catch (Exception)
            {
                throw new SecureSessionHolderException("Unknown session state");
            }
        }

    }
}
