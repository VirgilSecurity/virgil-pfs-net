using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Virgil.PFS.Client;
using Virgil.SDK.Client;
using Virgil.SDK.Cryptography;
using Virgil.SDK.Storage;

namespace Virgil.PFS
{
    internal abstract class KeyHolder
    {
        protected IKeyStorage keyStorage;
        protected ICrypto crypto;
        protected string ownerId;

        protected KeyHolder(ICrypto crypto, string ownerCardId)
        {
            this.crypto = crypto;
            this.keyStorage = new DefaultKeyStorage();
            this.ownerId = ownerCardId;
        }

        protected abstract string StoragePrefix();

        private string OwnerPrefixName()
        {
            return $"v.{this.ownerId}";
        }

        private string StoragePrefixForCurrentOwner()
        {
            return this.OwnerPrefixName() + this.StoragePrefix();
        }
        protected string PathToKey(string name)
        {
            return this.StoragePrefixForCurrentOwner() + name;
        }

        public void SaveKeyByName(IPrivateKey privateKey, string name)
        {
            var keyEntry = new KeyEntry
            {
                Name = this.PathToKey(name),
                Value = crypto.ExportPrivateKey(privateKey)
            };
            this.keyStorage.Store(keyEntry);
        }

        public void RemoveKey(string cardId)
        {
            this.keyStorage.Delete(this.PathToKey(cardId));
        }

        public bool IsKeyExist(string cardId)
        {
            return this.keyStorage.Exists(this.PathToKey(cardId));
        }

        public IPrivateKey LoadKeyByName(string cardId)
        {
            var key = this.keyStorage.Load(this.PathToKey(cardId));
            return this.crypto.ImportPrivateKey(key.Value);
        }

        public Dictionary<string, IPrivateKey> AllKeys()
        {
            var keyPaths = Array.FindAll(this.keyStorage.Names(), s => s.Contains(this.StoragePrefixForCurrentOwner()));
            var keys = new List<IPrivateKey>();
            var result = new Dictionary<string, IPrivateKey>();
            foreach (var keyPath in keyPaths)
            {
                var key = this.keyStorage.Load(keyPath);
                var privateKey = this.crypto.ImportPrivateKey(key.Value);
                var cardId = key.Name.Split(new string[] { this.StoragePrefix()}, StringSplitOptions.None).Last();
                result.Add(cardId, privateKey);
            }
            return result;
        }

        public bool HasKeys()
        {
            var keyPaths = Array.FindAll(this.keyStorage.Names(), s => s.Contains(this.StoragePrefixForCurrentOwner()));
            return (keyPaths.Length > 0);
        }

    }
}
