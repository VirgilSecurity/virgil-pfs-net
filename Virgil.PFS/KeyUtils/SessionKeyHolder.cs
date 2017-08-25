using System;
using System.Linq;
using System.Text;
using Virgil.PFS.Client;
using Virgil.SDK.Storage;

namespace Virgil.PFS.KeyUtils
{
    internal class SessionKeyHolder
    {
        protected IKeyStorage keyStorage;
        protected string ownerId;
        protected string expiredFieldName = "expired_at";

        public SessionKeyHolder(string ownerCardId)
        {
            this.keyStorage = new DefaultKeyStorage();
            this.ownerId = ownerCardId;
        }

        private string OwnerPrefixName()
        {
            return $"v.{this.ownerId}";
        }

        protected string StoragePrefixForCurrentOwner()
        {
            return this.OwnerPrefixName() + ".ss.";
        }
        protected string PathToKey(string name)
        {
            return this.StoragePrefixForCurrentOwner() + name;
        }

        public void SaveKeyByName(SessionKey sessionKey, string sessionId)
        {
            var keyEntry = new KeyEntry
            {
                Name = this.PathToKey(sessionId),
                Value = sessionKey.EncryptionKey.Concat(sessionKey.DecryptionKey).ToArray()
            };
            this.keyStorage.Store(keyEntry);
        }

        public void RemoveKey(string sessionId)
        {
            this.keyStorage.Delete(this.PathToKey(sessionId));
        }

        public bool IsKeyExist(string sessionId)
        {
            return this.keyStorage.Exists(this.PathToKey(sessionId));
        }

        public SessionKey LoadKeyByName(string sessionId)
        {
            var entry = this.keyStorage.Load(this.PathToKey(sessionId));
            var keyLength = entry.Value.Length / 2;
            var sessionKey = new SessionKey()
            {
                EncryptionKey = entry.Value.Take(keyLength).ToArray(),
                DecryptionKey = entry.Value.Skip(keyLength).Take(entry.Value.Length).ToArray()
            };

            return sessionKey;
        }
        /*
        public Dictionary<string, KeyInfo> AllKeys()
        {
            var keyPaths = Array.FindAll(this.keyStorage.Names(), s => s.Contains(this.StoragePrefixForCurrentOwner()));
            var keys = new List<IPrivateKey>();
            var result = new Dictionary<string, KeyInfo>();
            foreach (var keyPath in keyPaths)
            {
                var key = this.keyStorage.Load(keyPath);
                var keyInfo = new KeyInfo()
                {
                    PrivateKey = this.crypto.ImportPrivateKey(key.Value),
                    ExpiredAt = (key.MetaData[this.expiredFieldName] == null) ?
                        null :
                        GetDateTime(key.MetaData[this.expiredFieldName])
                };
                var cardId = key.Name.Split(new string[] { this.StoragePrefix() }, StringSplitOptions.None).Last();
                result.Add(cardId, keyInfo);
            }
            return result;
        }

        public void RemoveAllKeys()
        {
            var keyPaths = Array.FindAll(this.keyStorage.Names(), s => s.Contains(this.StoragePrefixForCurrentOwner()));
            foreach (var keyPath in keyPaths)
            {
                this.keyStorage.Delete(keyPath);
            }
        }*/

        public bool HasKeys()
        {
            var keyPaths = Array.FindAll(this.keyStorage.Names(), s => s.Contains(this.StoragePrefixForCurrentOwner()));
            return (keyPaths.Length > 0);
        }
        /*
        protected static string GetTimestamp(DateTime value)
        {
            return value.ToString("yyyyMMddHHmmssfff");
        }

        protected static DateTime? GetDateTime(string timestamp)
        {
            return DateTime.ParseExact(timestamp, "yyyyMMddHHmmssfff", null);
        }*/

    }

  /*  internal class KeyInfo
    {
        public IPrivateKey PrivateKey;
        public DateTime? ExpiredAt;
    }

}*/
}
