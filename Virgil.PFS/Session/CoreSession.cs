using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Virgil.Crypto.Pfs;
using Virgil.PFS.Client;
using Virgil.PFS.Exceptions;
using Virgil.PFS.KeyUtils;
using Virgil.SDK;

namespace Virgil.PFS.Session
{
    public class CoreSession
    {
        protected VirgilPFS pfs;
        private InitialMessageGenerator initialMessageGenerator;
        private DateTime expiredAt;
        private DateTime createdAt;

        public DateTime ExpiredAt()
        {
            return expiredAt;
        }

        public DateTime CreatedAt()
        {
            return createdAt;
        }
        public CoreSession(DateTime expiredAt, InitialMessageGenerator initialMessageGenerator)
        {
            this.pfs = new VirgilPFS();
            this.expiredAt = expiredAt;
            this.createdAt = DateTime.Now;
            this.initialMessageGenerator = initialMessageGenerator;
        }
        public CoreSession(byte[] sessionId,
            byte[] encryptionKey,
            byte[] decryptionKey,
            byte[] additionalData,
            DateTime createdAt,
            DateTime expiredAt
            ) : this(expiredAt, null)
        {
            var data = new byte[] { };
            if (additionalData != null)
            {
                data = additionalData;
            }
            var session = new VirgilPFSSession(sessionId, encryptionKey, decryptionKey, data);
            this.pfs.SetSession(session);
            this.createdAt = createdAt;
        }

        public CoreSession(VirgilPFSSession session, DateTime expiredAt, InitialMessageGenerator initialMessageGenerator)
        {
            this.pfs.SetSession(session);
            this.initialMessageGenerator = initialMessageGenerator;
            this.expiredAt = expiredAt;
        }

        //for initiator
        public CoreSession(VirgilPFSPublicKey recipientPfsPublicKey,
            VirgilPFSPublicKey recipientPfsLtPublicKey,
            VirgilPFSPublicKey recipientPfsOtPublicKey,
            VirgilPFSInitiatorPrivateInfo pfsInitiatorPrivateInfo,
            byte[] additionalData,
            InitialMessageGenerator initialMessageGenerator,
            DateTime expiredAt
        ) : this(expiredAt, initialMessageGenerator)
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

        //for responder
        public CoreSession(
            VirgilPFSPrivateKey pfsOtPrivateKey,
            VirgilPFSPublicKey initiatorIdentityPublicKey,
            VirgilPFSPublicKey initiatorEphPublicKey,
            VirgilPFSPrivateKey pfsPrivateKey,
            VirgilPFSPrivateKey pfsLtPrivateKey,
            byte[] additionalData,
            DateTime expiredAt
            ) : this(expiredAt, null)
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

        private string Decrypt(Message msg)
        {
            var pfsEncryptedMessage = new VirgilPFSEncryptedMessage(
                msg.SessionId,
                msg.Salt,
                msg.CipherText);
            var msgData = pfs.Decrypt(pfsEncryptedMessage);

            return VirgilBuffer.From(msgData).ToString(StringEncoding.Utf8);
        }


        public string Decrypt(string encryptedMessage)
        {
            this.Validate();

            if (MessageHelper.IsInitialMessage(encryptedMessage))
            {
                var initialMessage = MessageHelper.ExtractInitialMessage(encryptedMessage);
                return this.Decrypt(initialMessage);
            }
            else
            {
                var message = MessageHelper.ExtractMessage(encryptedMessage);
                return this.Decrypt(message);
            }
        }

        private string Decrypt(InitialMessage encryptedMessage)
        {
            var message = new Message()
            {
                SessionId = this.GetId(),
                CipherText = encryptedMessage.CipherText,
                Salt = encryptedMessage.Salt
            };
            return this.Decrypt(message);

        }

        public string Encrypt(string message)
        {
            this.Validate();

            if (this.initialMessageGenerator != null)
            {
                var initialMessageEncrypted = EncryptInitialMessage(message);
                return JsonSerializer.Serialize(initialMessageEncrypted);
            }
            else
            {
                return JsonSerializer.Serialize(EncryptMessage(message));
            }
        }

        private InitialMessage EncryptInitialMessage(string message)
        {
            var msg = EncryptMessage(message);
            var initialMessage = this.initialMessageGenerator.Generate(msg);
            this.initialMessageGenerator = null;
            return initialMessage;
        }

        private Message EncryptMessage(string message)
        {
            var msgData = VirgilBuffer.From(message).GetBytes();
            var encryptedMessage = this.pfs.Encrypt(msgData);
            var msg = new Message()
            {
                Salt = encryptedMessage.GetSalt(),
                CipherText = encryptedMessage.GetCipherText(),
                SessionId = encryptedMessage.GetSessionIdentifier()
            };
            return msg;
        }


        public bool IsInitialized()
        {
            return (this.pfs.GetSession() != null && !this.pfs.GetSession().IsEmpty());
        }

        public SessionKey GetKey()
        {
            this.Validate();
            return new SessionKey()
            {
                DecryptionKey = this.pfs.GetSession().GetDecryptionSecretKey(),
                EncryptionKey = this.pfs.GetSession().GetEncryptionSecretKey()
            };
        }

        public byte[] GetId()
        {
            this.Validate();
            return this.pfs.GetSession().GetIdentifier();
        }

        public byte[] GetAdditionalData()
        {
            this.Validate();
            return this.pfs.GetSession().GetAdditionalData();
        }

        private void Validate()
        {
            if (!this.IsInitialized())
            {
                throw new SecureSessionException("Session is not initialized!");
            }
        }
    }
}
