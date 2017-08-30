using System;
using System.Linq;
using System.Text;
using Virgil.PFS.Client;
using Virgil.SDK.Storage;

namespace Virgil.PFS.KeyUtils
{
    internal class SessionKeyStorage
    {
        protected IKeyStorage keyStorage;
        protected string ownerId;
        protected string expiredFieldName = "expired_at";

        public SessionKeyStorage(string ownerCardId)
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

        public void SaveKeyByName(SessionKey sessionKey, string InterlocutorCardId)
        {
            var keyEntry = new KeyEntry
            {
                Name = this.PathToKey(InterlocutorCardId),
                Value = sessionKey.EncryptionKey.Concat(sessionKey.DecryptionKey).ToArray()
            };
            this.keyStorage.Store(keyEntry);
        }

        public void RemoveKey(string InterlocutorCardId)
        {
            this.keyStorage.Delete(this.PathToKey(InterlocutorCardId));
        }

        public bool IsKeyExist(string InterlocutorCardId)
        {
            return this.keyStorage.Exists(this.PathToKey(InterlocutorCardId));
        }

        public SessionKey LoadKeyByName(string InterlocutorCardId)
        {
            var entry = this.keyStorage.Load(this.PathToKey(InterlocutorCardId));
            var keyLength = entry.Value.Length / 2;
            var sessionKey = new SessionKey()
            {
                EncryptionKey = entry.Value.Take(keyLength).ToArray(),
                DecryptionKey = entry.Value.Skip(keyLength).Take(entry.Value.Length).ToArray()
            };

            return sessionKey;
        }

        public bool HasKeys()
        {
            var keyPaths = Array.FindAll(this.keyStorage.Names(), s => s.Contains(this.StoragePrefixForCurrentOwner()));
            return (keyPaths.Length > 0);
        }

    }
}
