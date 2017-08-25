using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Virgil.Crypto.Pfs;
using Virgil.PFS.Client;
using Virgil.PFS.KeyUtils;
using Virgil.SDK;

namespace Virgil.PFS.Session
{
    public class CoreSession : ISession
    {
        protected VirgilPFS pfs;

        private CoreSession()
        {
            this.pfs = new VirgilPFS();
        }
        public CoreSession(byte[] sessionId,
            byte[] encryptionKey,
            byte[] decryptionKey,
            byte[] additionalData
            ) : this()
        {
            var session = new VirgilPFSSession(sessionId, encryptionKey, decryptionKey, additionalData);
            this.pfs.SetSession(session);

        }

        public CoreSession(VirgilPFSPublicKey recipientPfsPublicKey,
            VirgilPFSPublicKey recipientPfsLtPublicKey,
            VirgilPFSPublicKey recipientPfsOtPublicKey,
            VirgilPFSInitiatorPrivateInfo pfsInitiatorPrivateInfo,
            byte[] additionalData
        ) : this()
        {
            VirgilPFSResponderPublicInfo pfsInitiatorPublicInfo = null;
            if (recipientPfsOtPublicKey != null)
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
            if (additionalData == null)
            {
                this.pfs.StartInitiatorSession(pfsInitiatorPrivateInfo, pfsInitiatorPublicInfo);
            }
            else
            {
                this.pfs.StartInitiatorSession(pfsInitiatorPrivateInfo, pfsInitiatorPublicInfo, additionalData);
            }
        }

        public CoreSession(
            VirgilPFSPrivateKey pfsOtPrivateKey,
            VirgilPFSPublicKey initiatorIdentityPublicKey,
            VirgilPFSPublicKey initiatorEphPublicKey,
            VirgilPFSPrivateKey pfsPrivateKey,
            VirgilPFSPrivateKey pfsLtPrivateKey,
            byte[] additionalData
            ) : this()
        {
            var initiatorPublicInfo = new VirgilPFSInitiatorPublicInfo(initiatorIdentityPublicKey, initiatorEphPublicKey);
            VirgilPFSResponderPrivateInfo responderPrivateInfo = null;
            responderPrivateInfo = (pfsOtPrivateKey == null) ?
                new VirgilPFSResponderPrivateInfo(pfsPrivateKey, pfsLtPrivateKey) :
                new VirgilPFSResponderPrivateInfo(pfsPrivateKey, pfsLtPrivateKey, pfsOtPrivateKey);

            if (additionalData == null)
            {
                this.pfs.StartResponderSession(responderPrivateInfo, initiatorPublicInfo);
            }
            else
            {
                this.pfs.StartResponderSession(responderPrivateInfo, initiatorPublicInfo, additionalData);
            }

        }


        public string Decrypt(string encryptedMessage)
        {
            throw new NotImplementedException();
        }

        public string Decrypt(Message msg)
        {
            var pfsEncryptedMessage = new VirgilPFSEncryptedMessage(
                msg.SessionId,
                msg.Salt,
                msg.CipherText);
            var msgData = pfs.Decrypt(pfsEncryptedMessage);

            return VirgilBuffer.From(msgData).ToString(StringEncoding.Utf8);
        }

        public string Encrypt(string message)
        {
            var msgData = VirgilBuffer.From(message).GetBytes();
            var encryptedMessage = this.pfs.Encrypt(msgData);
            var msg = new Message()
            {
                Salt = encryptedMessage.GetSalt(),
                CipherText = encryptedMessage.GetCipherText(),
                SessionId = encryptedMessage.GetSessionIdentifier()
            };

            return JsonSerializer.Serialize(msg);
        }

        public bool IsInitialized()
        {
            return (this.pfs.GetSession() != null && !this.pfs.GetSession().IsEmpty());
        }

        public SessionKey GetKey()
        {
           return  new SessionKey()
            {
                DecryptionKey = this.pfs.GetSession().GetDecryptionSecretKey(),
                EncryptionKey = this.pfs.GetSession().GetEncryptionSecretKey()
            };
        }

        public byte[] GetSessionId()
        {
            return this.pfs.GetSession().GetIdentifier();
        }

        public string GetSessionIdBase64()
        {
            return Convert.ToBase64String(this.GetSessionId());
        }
    }
}
