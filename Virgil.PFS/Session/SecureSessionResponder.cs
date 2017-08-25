using Virgil.PFS.Session;

namespace Virgil.PFS
{
    using System;
    using Virgil.Crypto.Pfs;
    using Virgil.PFS.Exceptions;
    using Virgil.SDK.Client;
    using Virgil.SDK.Cryptography;

    public class SecureSessionResponder : SecureSession
    {
        private CardModel initiatorIdentityCard;

        public SecureSessionResponder(
            ICrypto crypto,
            IPrivateKey myPrivateKey,
            CardModel myIdentityCard,
            CardModel initiatorIdentityCard,
            byte[] additionalData,
            SecureChatKeyHelper keyHelper,
            SecureSessionHelper sessionHelper,
            DateTime expiredAt,
            bool recovered = false) :
            base(crypto, myPrivateKey, recovered, expiredAt, keyHelper, sessionHelper, initiatorIdentityCard.Id, additionalData)
        {
            this.initiatorIdentityCard = initiatorIdentityCard;
        }

        public SecureSessionResponder(ICrypto crypto,
            IPrivateKey myPrivateKey,
            CardModel myIdentityCard,
            CardModel initiatorIdentityCard,
            byte[] additionalData,
            SecureChatKeyHelper keyHelper,
            SecureSessionHelper sessionHelper,
            byte[] initiatorEphPublicKeyData,
            string responderLtcId,
            string responderOtcId,
            DateTime expiredAt) :
            this(crypto, myPrivateKey, myIdentityCard, initiatorIdentityCard, additionalData,
            keyHelper, sessionHelper, expiredAt, true)
        {
            this.InitializeSession(initiatorEphPublicKeyData, responderLtcId, responderOtcId);
        }

        internal string Decrypt(InitialMessage encryptedMessage)
        {
            if (!this.IsInitialized())
            {
                this.InitializeSession(encryptedMessage);
            }
            var message = new Message()
            {
                SessionId = this.CoreSession.GetSessionId(),
                CipherText = encryptedMessage.CipherText,
                Salt = encryptedMessage.Salt
            };
            return this.Decrypt(message);

        }

        private void InitializeSession(InitialMessage message)
        {
            this.ValidateInitiatorEphPublicKey(message);
            this.ValidateInitiatorIdentityCardId(message);

            this.InitializeSession(message.EphPublicKey, message.ResponderLtcId, message.ResponderOtcId);
        }

        private void ValidateInitiatorIdentityCardId(InitialMessage message)
        {
            if (message.InitiatorIcId != this.initiatorIdentityCard.Id)
            {
                throw new SecureSessionResponderException(
                    "Initiator identity card id for this session and InitiationMessage doesn't match.");
            }
        }


        private void ValidateInitiatorEphPublicKey(InitialMessage message)
        {
            var initiatorPublicKey =
                this.crypto.ImportPublicKey(this.initiatorIdentityCard.SnapshotModel.PublicKeyData);
            if (!this.crypto.Verify(message.EphPublicKey, message.EphPublicKeySignature, initiatorPublicKey))
            {
                throw new SecureSessionResponderException("Error validating initiator signature.");
            }
        }

        private void InitializeSession(byte[] initiatorEphPublicKeyData, string responderLtcId, string responderOtcId)
        {
            var myPrivateKeyData = this.crypto.ExportPrivateKey(this.myPrivateKey);
            var pfsPrivateKey = new VirgilPFSPrivateKey(myPrivateKeyData);
            var myLtPrivateKey = this.crypto.ExportPrivateKey(this.keyHelper.LtKeyHolder().LoadKeyByName(responderLtcId));
            var pfsLtPrivateKey = new VirgilPFSPrivateKey(myLtPrivateKey);

            var initiatorIdentityPublicKey = new VirgilPFSPublicKey(initiatorIdentityCard.SnapshotModel.PublicKeyData);
            var initiatorEphPublicKey = new VirgilPFSPublicKey(initiatorEphPublicKeyData);
            VirgilPFSPrivateKey pfsOtPrivateKey = null;
            if (responderOtcId != null)
            {
                var myOtPrivateKeyData = this.crypto.ExportPrivateKey(
                    this.keyHelper.OtKeyHolder().LoadKeyByName(responderOtcId));
                pfsOtPrivateKey = new VirgilPFSPrivateKey(myOtPrivateKeyData);
            }

            this.CoreSession = new CoreSession(pfsOtPrivateKey,
                initiatorIdentityPublicKey,
                initiatorEphPublicKey,
                pfsPrivateKey,
                pfsLtPrivateKey,
                additionalData);


            if (!this.isRecovered)
            {
                if (this.keyHelper.OtKeyHolder().IsKeyExist(responderOtcId))
                {
                    this.keyHelper.OtKeyHolder().RemoveKey(responderOtcId);
                }
                this.SaveCoreSessionData();
            }
        }

        public override string Decrypt(string encryptedMessage)
        {
            if (MessageHelper.IsInitialMessage(encryptedMessage))
            {
                var initialMessage = MessageHelper.ExtractInitialMessage(encryptedMessage);
                return this.Decrypt(initialMessage);
            }
            else
            {
                if (!this.IsInitialized())
                {
                    throw new SecureSessionResponderException("Session is not initialized!");
                }
                var message = MessageHelper.ExtractMessage(encryptedMessage);
                return base.Decrypt(message);
            }
        }


    }
}
