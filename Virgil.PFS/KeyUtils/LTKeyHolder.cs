using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Virgil.SDK.Cryptography;
using Virgil.SDK.Storage;

namespace Virgil.PFS
{
    internal class LtKeyHolder : KeyHolder
    {
        private readonly int ltPrivateKeyLifeDays;
        public LtKeyHolder(ICrypto crypto, string ownerCardId, int ltKeyLifeDays) : base(crypto, ownerCardId)
        {
            this.ltPrivateKeyLifeDays = ltKeyLifeDays;
        }

        protected override string StoragePrefix()
        {
            return ".lt.";
        }

        public new void SaveKeyByName(IPrivateKey privateKey, string name)
        {
            var meta = new Dictionary<string, string>
            {
                //lt key should live one extra day
                {expiredFieldName, GetTimestamp(DateTime.Now.AddDays(this.ltPrivateKeyLifeDays + 1))}
            };
            var keyEntry = new KeyEntry
            {
                Name = this.PathToKey(name),
                Value = crypto.ExportPrivateKey(privateKey),
                MetaData = meta
            };
            this.keyStorage.Store(keyEntry);
        }

        public bool IsWaitingForNewKey()
        {
            return !this.AllKeys().Values.Any(x => (DateTime.Now < ((DateTime) x.ExpiredAt).AddDays(-1)));
        }
        public void RemoveExpiredKeys()
        {
            var paths = Array.FindAll(this.keyStorage.Names(), s => s.Contains(this.StoragePrefixForCurrentOwner()));

            foreach (var path in paths)
            {
                var key = this.keyStorage.Load(path);
                var expiredAt = GetDateTime(key.MetaData[this.expiredFieldName]);
                
                if (DateTime.Now >= expiredAt)
                {
                    this.keyStorage.Delete(path);
                }
            }
        }
    }
}