using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Virgil.Crypto.Pfs;
using Virgil.PFS.Client;
using Virgil.SDK.Client;
using Virgil.SDK.Cryptography;

namespace Virgil.PFS
{
    public class SecureSessionResponder : SecureSession
    {
        private CardModel initiatorIdentityCard;
        private SecureChatKeyHelper keyHelper;
        private SecureSessionHelper sessionHelper;

        public SecureSessionResponder(ICrypto crypto, IPrivateKey myPrivateKey, CardModel myIdentityCard,
            CardModel initiatorIdentityCard, byte[] additionalData,
            SecureChatKeyHelper keyHelper, bool recovered = false) :
            base(crypto, myPrivateKey, recovered, additionalData)
        {
            this.initiatorIdentityCard = initiatorIdentityCard;
            this.keyHelper = keyHelper;
            this.sessionHelper = new SecureSessionHelper(myIdentityCard.Id);
        }



        internal string Decrypt(InitialMessage encryptedMessage)
        {
            if (!this.IsInitialized())
            {
                this.InitializeSession(encryptedMessage);
            }
            var sessionId = this.pfs.GetSession().GetIdentifier();
            var message = new Message()
            {
                SessionId = sessionId,
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
                throw new Exception("Initiator identity card id for this session and InitiationMessage doesn't match.");
            }
        }


        private void ValidateInitiatorEphPublicKey(InitialMessage message)
        {
            var initiatorPublicKey =
                this.crypto.ImportPublicKey(this.initiatorIdentityCard.SnapshotModel.PublicKeyData);
            if (!this.crypto.Verify(message.EphPublicKey, message.EphPublicKeySignature, initiatorPublicKey))
            {
                throw new Exception("Error validating initiator signature.");
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
            var session = this.StartResponderSession(pfsOtPrivateKey, 
                initiatorIdentityPublicKey, 
                initiatorEphPublicKey, 
                pfsPrivateKey, 
                pfsLtPrivateKey);

            if (!this.isRecovered)
            {
                SaveSessionState(initiatorEphPublicKeyData, responderLtcId, responderOtcId, session);
            }
        }

        private VirgilPFSSession StartResponderSession(VirgilPFSPrivateKey pfsOtPrivateKey, 
            VirgilPFSPublicKey initiatorIdentityPublicKey,
            VirgilPFSPublicKey initiatorEphPublicKey, 
            VirgilPFSPrivateKey pfsPrivateKey, 
            VirgilPFSPrivateKey pfsLtPrivateKey)
        {
            var initiatorPublicInfo = new VirgilPFSInitiatorPublicInfo(initiatorIdentityPublicKey, initiatorEphPublicKey);
            VirgilPFSResponderPrivateInfo responderPrivateInfo = null;
            responderPrivateInfo = (pfsOtPrivateKey == null) ? 
                new VirgilPFSResponderPrivateInfo(pfsPrivateKey, pfsLtPrivateKey) : 
                new VirgilPFSResponderPrivateInfo(pfsPrivateKey, pfsLtPrivateKey, pfsOtPrivateKey);

            if (this.additionalData == null)
            {
                return this.pfs.StartResponderSession(responderPrivateInfo, initiatorPublicInfo);
            }
            else
            {
                return this.pfs.StartResponderSession(responderPrivateInfo, initiatorPublicInfo, this.additionalData);
            }
        }


        private void SaveSessionState(byte[] initiatorEphPublicKeyData, string responderLtcId, string responderOtcId,
            VirgilPFSSession session)
        {
            var sessionId = session.GetIdentifier();
            var sessionState = new ResponderSessionState(sessionId,
                this.createdAt,
                this.expiredAt,
                this.additionalData,
                initiatorEphPublicKeyData,
                this.initiatorIdentityCard,
                responderLtcId,
                responderOtcId);

            if (this.sessionHelper.ExistSessionState(this.initiatorIdentityCard.Id))
            {
                this.sessionHelper.DeleteSessionState(this.initiatorIdentityCard.Id);
            }

            this.sessionHelper.SaveSessionState(sessionState, this.initiatorIdentityCard.Id);
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
                    throw new Exception("Session is not initialized!"); //todo VirgilSession exception
                }
                var message = MessageHelper.ExtractMessage(encryptedMessage);
                return base.Decrypt(message);
            }
        }


    }
}
