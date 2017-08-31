using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Virgil.SDK.Cryptography;

namespace Virgil.PFS
{
    internal class OtKeyStorage : KeyStorage
    {
        private int otLifeDaysAfterExhausting = 1;
        public OtKeyStorage(ICrypto crypto, string ownerCardId) : base(crypto, ownerCardId)
        {
        }

        protected override string StoragePrefix()
        {
            return ".ot.";
        }

        public void SetUpExpiredAt(string cardId)
        {
            var keyEntry = this.coreKeyStorage.Load(this.PathToKey(cardId));
            this.coreKeyStorage.Delete(this.PathToKey(cardId));
            keyEntry.MetaData = new Dictionary<string, string>
            {
                {expiredFieldName, GetTimestamp(DateTime.Now.AddDays(this.otLifeDaysAfterExhausting))}
            };
            this.coreKeyStorage.Store(keyEntry);
        }
    }
}
