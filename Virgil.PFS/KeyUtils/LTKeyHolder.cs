using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Virgil.SDK.Cryptography;
using Virgil.SDK.Storage;

namespace Virgil.PFS
{
    internal class LtKeyHolder : KeyHolder
    {
        private readonly int ltPrivateKeyLifeDays;
        private string expiredFieldName = "expired_at";
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
                {expiredFieldName, GetTimestamp(DateTime.Now.AddDays(this.ltPrivateKeyLifeDays))}
            };
            var keyEntry = new KeyEntry
            {
                Name = this.PathToKey(name),
                Value = crypto.ExportPrivateKey(privateKey),
                MetaData = meta
            };
            this.keyStorage.Store(keyEntry);
        }


        private static string GetTimestamp(DateTime value)
        {
            return value.ToString("yyyyMMddHHmmssfff");
        }

        private static DateTime GetDateTime(string timestamp)
        {
            return DateTime.ParseExact(timestamp, "yyyyMMddHHmmssfff", null);
        }

        public string[] RemoveExpiredKeys()
        {
            var cardIds = new List<string>();

            var paths = Array.FindAll(this.keyStorage.Names(), s => s.Contains(this.StoragePrefixForCurrentOwner()));

            foreach (var path in paths)
            {
                var key = this.keyStorage.Load(path);
                var expiredAt = GetDateTime(key.MetaData[this.expiredFieldName]);

                if (DateTime.Now > expiredAt)
                {
                    cardIds.Add(key.Name);
                    this.keyStorage.Delete(path);
                }
            }
            return cardIds.ToArray();
        }
    }
}
