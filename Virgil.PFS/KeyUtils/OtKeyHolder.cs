using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Virgil.SDK.Cryptography;

namespace Virgil.PFS
{
    internal class OtKeyHolder : KeyHolder
    {
        private int otLifeDaysAfterExhausting = 1;
        public OtKeyHolder(ICrypto crypto, string ownerCardId) : base(crypto, ownerCardId)
        {
        }

        protected override string StoragePrefix()
        {
            return ".ot.";
        }

        public void SetUpExpiredAt(string cardId)
        {
            var keyEntry = this.keyStorage.Load(this.PathToKey(cardId));
            this.keyStorage.Delete(this.PathToKey(cardId));
            keyEntry.MetaData = new Dictionary<string, string>
            {
                {expiredFieldName, GetTimestamp(DateTime.Now.AddDays(this.otLifeDaysAfterExhausting))}
            };
            this.keyStorage.Store(keyEntry);
        }
    }
}
