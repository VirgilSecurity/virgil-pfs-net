using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Virgil.SDK.Cryptography;
using Virgil.Crypto.Pfs;
using Virgil.PFS.Client;
using Virgil.SDK;
using Virgil.SDK.Client;

namespace Virgil.PFS
{
    public abstract class SecureSession
    {
        protected ICrypto crypto;
        protected IPrivateKey myPrivateKey;
        protected DateTime createdAt;
        protected DateTime expiredAt;
        protected VirgilPFS pfs;
        protected bool isRecovered;
        protected byte[] additionalData;



        public SecureSession(ICrypto crypto, 
            IPrivateKey myPrivateKey, 
            bool recovered, 
            DateTime expiredAt,
            byte[] additionalData)
        {
            this.crypto = crypto;
            this.myPrivateKey = myPrivateKey;
            this.createdAt = DateTime.Now;
            this.expiredAt = expiredAt;
            this.pfs = new VirgilPFS();
            this.isRecovered = recovered;
            this.additionalData = additionalData;
        }


        public bool IsRecovered()
        {
            return this.isRecovered;
        }
        public bool IsInitialized()
        {
            
            return (this.pfs.GetSession() != null && !this.pfs.GetSession().IsEmpty());
        }
        public bool IsExpired()
        {
            return (DateTime.Now > this.expiredAt);
        }

        public virtual string Encrypt(String message)
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




        public abstract string Decrypt(string encryptedMessage);


        protected string Decrypt(Message msg)
        {
            var pfsEncryptedMessage = new VirgilPFSEncryptedMessage(
                msg.SessionId,
                msg.Salt,
                msg.CipherText);
            var msgData = pfs.Decrypt(pfsEncryptedMessage);

            return VirgilBuffer.From(msgData).ToString(StringEncoding.Utf8);
        }

    }
}
