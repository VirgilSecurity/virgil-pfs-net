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
using Virgil.PFS.KeyUtils;
using Virgil.PFS.Session;
using Virgil.SDK;
using Virgil.SDK.Client;
using Virgil.PFS.Exceptions;

namespace Virgil.PFS.Session
{
    /*public abstract class SecureSession : ISession
    {
        protected ICrypto crypto;
        protected IPrivateKey myPrivateKey;
        protected DateTime createdAt;
        protected DateTime expiredAt;
        protected byte[] additionalData;
        protected SecureSessionHelper sessionHelper;
        protected SecureChatKeyHelper keyHelper;
        protected string InterlocutorCardId;
        protected CoreSession CoreSession;


        public SecureSession(ICrypto crypto, 
            IPrivateKey myPrivateKey, 
            DateTime expiredAt,
            string interlocutorCardId,
            byte[] additionalData)
        {
            this.crypto = crypto;
            this.myPrivateKey = myPrivateKey;
            this.createdAt = DateTime.Now;
            this.expiredAt = expiredAt;
            this.InterlocutorCardId = interlocutorCardId;
            this.additionalData = additionalData;
        }

        public DateTime CreatedAt()
        {
            return this.createdAt;
        }

        public DateTime ExpiredAt()
        {
            return this.expiredAt;
        }

        public bool IsInitialized()
        {
            return (this.CoreSession != null && this.CoreSession.IsInitialized());
        }
        public bool IsExpired()
        {
            return (DateTime.Now > this.expiredAt);
        }

        public virtual string Encrypt(string message)
        {
            return this.CoreSession.Encrypt(message);
        }

        public abstract string Decrypt(string encryptedMessage);

        public string Decrypt(Message msg)
        {
            return this.CoreSession.Decrypt(msg);
        }

        public byte[] GetId()
        {
            if (this.IsInitialized())
            {
                return this.CoreSession.GetId();
            }
            return null;
        }

        public SessionKey GetKey()
        {
            if (this.IsInitialized())
            {
                return this.CoreSession.GetKey();
            }
            return null;

        }

        public byte[] GetAdditionalData()
        {
            return this.CoreSession.GetAdditionalData();
        }

        protected void Validate()
        {
            if (!this.IsInitialized())
            {
                throw new SecureSessionException("Secure Session is not initialized!");
            }
        }

    }*/
}
