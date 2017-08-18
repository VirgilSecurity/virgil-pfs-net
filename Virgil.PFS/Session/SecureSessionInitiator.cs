using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Virgil.Crypto.Pfs;
using Virgil.PFS.Client;
using Virgil.PFS.KeyUtils;
using Virgil.SDK;
using Virgil.SDK.Client;
using Virgil.SDK.Cryptography;

namespace Virgil.PFS
{
    class SecureSessionInitiator : SecureSession
    {

        private CardModel myIdentityCard;
        private IPrivateKey myEphPrivateKey;
        private CredentialsModel recipientCredentials;
        private string myEphKeyName;
        private CardModel recipientIdentityCard;
        private SecureSessionHelper sessionHelper;

        public SecureSessionInitiator(ICrypto crypto,
            IPrivateKey myPrivateKey,
            CardModel myIdentityCard,
            IPrivateKey myEphPrivateKey,
            string myEphKeyName,
            CredentialsModel recipientCredentials,
            CardModel recipientIdentityCard,
            byte[] additionalData,
            SecureSessionHelper sessionHelper,
            DateTime expiredAt,
            bool recovered = false
            )
            : base(crypto, myPrivateKey, recovered, expiredAt, additionalData)
        {
            this.myIdentityCard = myIdentityCard;
            this.myEphPrivateKey = myEphPrivateKey;
            this.myEphKeyName = myEphKeyName;
            this.recipientCredentials = recipientCredentials;
            this.recipientIdentityCard = recipientIdentityCard;
            // todo change
            //this.sessionHelper = new SecureSessionHelper(this.myIdentityCard.Id);
            this.sessionHelper = sessionHelper;
            if (recovered)
            {
            this.InitializeSession();

            }
        }
        private void InitializeSession()
        {
            var myPrivateKeyData = crypto.ExportPrivateKey(this.myPrivateKey);
            var pfsPrivateKey = new VirgilPFSPrivateKey(myPrivateKeyData);

            var ephPrivateKeyData = crypto.ExportPrivateKey(this.myEphPrivateKey);
            var pfsEphPrivateKey = new VirgilPFSPrivateKey(ephPrivateKeyData);

            var pfsInitiatorPrivateInfo = new VirgilPFSInitiatorPrivateInfo(pfsPrivateKey, pfsEphPrivateKey);

            var recipientPfsPublicKey = new VirgilPFSPublicKey(this.recipientIdentityCard.SnapshotModel.PublicKeyData);
            var recipientPfsLtPublicKey = new VirgilPFSPublicKey(this.recipientCredentials.LTCard.SnapshotModel.PublicKeyData);
            VirgilPFSPublicKey recipientPfsOtPublicKey = null;
            if (this.recipientCredentials.OTCard != null)
            {
                recipientPfsOtPublicKey =
                    new VirgilPFSPublicKey(this.recipientCredentials.OTCard.SnapshotModel.PublicKeyData);
            }

            var session = this.StartInitiatorSession(recipientPfsPublicKey, 
                recipientPfsLtPublicKey, 
                recipientPfsOtPublicKey, 
                pfsInitiatorPrivateInfo);
            if (!this.isRecovered)
            {
                var keyHolder = new SessionKeyHolder(crypto, this.myIdentityCard.Id);
                keyHolder.SaveKeyByName(this.myEphPrivateKey, this.myEphKeyName);

                SaveSessionState(session);
            }
        }

        private VirgilPFSSession StartInitiatorSession(
            VirgilPFSPublicKey recipientPfsPublicKey,
            VirgilPFSPublicKey recipientPfsLtPublicKey,
            VirgilPFSPublicKey recipientPfsOtPublicKey,
            VirgilPFSInitiatorPrivateInfo pfsInitiatorPrivateInfo)
        {
            VirgilPFSResponderPublicInfo pfsInitiatorPublicInfo = null;
            if (this.recipientCredentials.OTCard != null)
            {
                pfsInitiatorPublicInfo = new VirgilPFSResponderPublicInfo(recipientPfsPublicKey,
                    recipientPfsLtPublicKey,
                    recipientPfsOtPublicKey);
            }
            else
            {
                pfsInitiatorPublicInfo = new VirgilPFSResponderPublicInfo(recipientPfsPublicKey,
                    recipientPfsLtPublicKey);
            }
            return (this.additionalData == null)
                ? this.pfs.StartInitiatorSession(pfsInitiatorPrivateInfo, pfsInitiatorPublicInfo)
                : this.pfs.StartInitiatorSession(pfsInitiatorPrivateInfo, pfsInitiatorPublicInfo, this.additionalData);
        }

        private void SaveSessionState(VirgilPFSSession session)
        {
            var sessionId = session.GetIdentifier();
            var sessionState = new InitiatorSessionState(sessionId,
                this.createdAt,
                this.expiredAt,
                this.additionalData,
                this.myEphKeyName,
                this.recipientIdentityCard,
                this.recipientCredentials.LTCard,
                this.recipientCredentials.OTCard
            );
            this.sessionHelper.SaveSessionState(sessionState, this.recipientIdentityCard.Id);
        }

        public override string Encrypt(String message)
        {
            var isFirstMessage = false;
            if (!this.IsInitialized())
            {
                isFirstMessage = true;
                this.InitializeSession();
            }

            string result = null;
            if (isFirstMessage)
            {
                var myEphPublicKey = this.crypto.ExtractPublicKey(this.myEphPrivateKey);
                var myEphPublicKeyData = this.crypto.ExportPublicKey(myEphPublicKey);
                var signForEphPublicKey = this.crypto.Sign(myEphPublicKeyData, this.myPrivateKey);

                var msgData = VirgilBuffer.From(message).GetBytes();
                var encryptedMessage = this.pfs.Encrypt(msgData);
                var msg = new Message()
                {
                    Salt = encryptedMessage.GetSalt(),
                    CipherText = encryptedMessage.GetCipherText(),
                    SessionId = encryptedMessage.GetSessionIdentifier()
                };
                var initialMsg = new InitialMessage()
                {
                    CipherText = msg.CipherText,
                    EphPublicKey = myEphPublicKeyData,
                    InitiatorIcId = this.myIdentityCard.Id,
                    EphPublicKeySignature = signForEphPublicKey,
                    ResponderIcId = this.recipientIdentityCard.Id,
                    ResponderLtcId = this.recipientCredentials.LTCard.Id,
                    ResponderOtcId = this.recipientCredentials.OTCard?.Id,
                    Salt = msg.Salt
                };
                result = JsonSerializer.Serialize(initialMsg);
            }
            else
            {
                result = base.Encrypt(message);
            }
            return result;
        }

        public override string Decrypt(string message)
        {
            if (!this.IsInitialized())
            {
                //todo SecureSessionExeption
                throw new Exception("Session is not initialized.");
            }
            var msg = MessageHelper.ExtractMessage(message);
            return base.Decrypt(msg);
        }
    }
}
