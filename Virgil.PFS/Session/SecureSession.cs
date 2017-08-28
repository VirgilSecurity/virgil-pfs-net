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
    public abstract class SecureSession : ISession
    {
        protected ICrypto crypto;
        protected IPrivateKey myPrivateKey;
        protected DateTime createdAt;
        protected DateTime expiredAt;
        protected bool isRecovered;
        protected byte[] additionalData;
        protected SecureSessionHelper sessionHelper;
        protected SecureChatKeyHelper keyHelper;
        protected string InterlocutorCardId;
        protected CoreSession CoreSession;


        public SecureSession(ICrypto crypto, 
            IPrivateKey myPrivateKey, 
            bool recovered, 
            DateTime expiredAt,
            SecureChatKeyHelper keyHelper,
            SecureSessionHelper sessionHelper,
            string interlocutorCardId,
            byte[] additionalData)
        {
            this.crypto = crypto;
            this.myPrivateKey = myPrivateKey;
            this.createdAt = DateTime.Now;
            this.expiredAt = expiredAt;
            this.isRecovered = recovered;
            this.sessionHelper = sessionHelper;
            this.keyHelper = keyHelper;
            this.InterlocutorCardId = interlocutorCardId;
            this.additionalData = additionalData;
        }


        public bool IsRecovered()
        {
            return this.isRecovered;
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

        protected void SaveCoreSessionData()
        {
            this.keyHelper.SessionKeyHolder().SaveKeyByName(this.CoreSession.GetKey(), InterlocutorCardId);
            
            var sessionState = new SessionState(
                this.CoreSession.GetSessionId(), 
                this.createdAt,
                this.expiredAt, 
                this.CoreSession.GetAdditionalData());
            this.sessionHelper.SaveSessionState(sessionState, InterlocutorCardId);
        }

        public string Decrypt(Message msg)
        {
            return this.CoreSession.Decrypt(msg);
        }

        public byte[] GetSessionId()
        {
            if (this.IsInitialized())
            {
                return this.CoreSession.GetSessionId();
            }
            return null;
        }

        protected void Validate()
        {
            if (!this.IsInitialized())
            {
                throw new SecureSessionException("Secure Session is not initialized!");
            }
        }

    }
}
