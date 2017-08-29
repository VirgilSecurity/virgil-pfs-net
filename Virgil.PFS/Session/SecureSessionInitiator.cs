using System.Text;
using Virgil.PFS.Session;

namespace Virgil.PFS.Session
{
    using System;
    using Virgil.Crypto.Pfs;
    using Virgil.PFS.Client;
    using Virgil.PFS.Exceptions;
    using Virgil.PFS.KeyUtils;
    using Virgil.SDK;
    using Virgil.SDK.Client;
    using Virgil.SDK.Cryptography;
    /*
    public class SecureSessionInitiator : SecureSession
    {

        private string myIdentityCardId;
        private IPrivateKey myEphPrivateKey;
        private CredentialsModel recipientCredentials;
        private CardModel recipientIdentityCard;

        public SecureSessionInitiator(ICrypto crypto,
            IPrivateKey myPrivateKey,
            string myIdentityCardId,
            IPrivateKey myEphPrivateKey,
            CredentialsModel recipientCredentials,
            CardModel recipientIdentityCard,
            byte[] additionalData,
            DateTime expiredAt
            )
            : base(crypto, myPrivateKey, expiredAt, recipientIdentityCard.Id, additionalData)
        {
            this.myIdentityCardId = myIdentityCardId;
            this.myEphPrivateKey = myEphPrivateKey;
            this.recipientCredentials = recipientCredentials;
            this.recipientIdentityCard = recipientIdentityCard;

            this.InitializeSession();

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

            this.CoreSession = new CoreSession(
                recipientPfsPublicKey,
                recipientPfsLtPublicKey,
                recipientPfsOtPublicKey,
                pfsInitiatorPrivateInfo,
                this.additionalData
                );
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

                var encryptedMessage = JsonSerializer.Deserialize<Message>(
                    this.CoreSession.Encrypt(message)
                    );

                var msg = new Message()
                {
                    Salt = encryptedMessage.Salt,
                    CipherText = encryptedMessage.CipherText,
                    SessionId = encryptedMessage.SessionId
                };
                var initialMsg = new InitialMessage()
                {
                    CipherText = msg.CipherText,
                    EphPublicKey = myEphPublicKeyData,
                    InitiatorIcId = this.myIdentityCardId,
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
            this.Validate();
            return this.CoreSession.Decrypt(message);
        } 
    }*/
}
