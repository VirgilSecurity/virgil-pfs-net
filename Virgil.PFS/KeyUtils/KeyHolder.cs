using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Virgil.PFS.Client;
using Virgil.SDK.Client;
using Virgil.SDK.Cryptography;
using Virgil.SDK.Storage;

namespace Virgil.PFS
{
    internal abstract class KeyHolder
    {
        private IKeyStorage keyStorage;
        private ICrypto crypto;
        private string ownerId;

        protected KeyHolder(ICrypto crypto, string ownerCardId)
        {
            this.crypto = crypto;
            this.keyStorage = new DefaultKeyStorage();
            this.ownerId = ownerCardId;
        }

        public abstract string GenerateKeyName(string id);

        private string OwnerPrefixName()
        {
            return $"v.{this.ownerId}.";
        }

        private string PathToKey(string name)
        {
            return this.OwnerPrefixName() + this.GenerateKeyName(name);
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




    }
}
