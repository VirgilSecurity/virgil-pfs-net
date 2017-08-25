using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Virgil.PFS.KeyUtils;
using Virgil.SDK.Cryptography;

namespace Virgil.PFS
{
    public class SecureChatKeyHelper
    {
        private ICrypto crypto;
        private string ownerCardId;
        private SessionKeyHolder sessionKeyHolder;
        private LtKeyHolder ltKeyHolder;
        private OtKeyHolder otKeyHolder;


        public SecureChatKeyHelper(ICrypto crypto, string ownerCardId, int ltKeyLifeDays)
        {
            this.crypto = crypto;
            this.ownerCardId = ownerCardId;
            this.sessionKeyHolder = new SessionKeyHolder(ownerCardId);
            this.ltKeyHolder = new LtKeyHolder(crypto, ownerCardId, ltKeyLifeDays);
            this.otKeyHolder = new OtKeyHolder(crypto, ownerCardId);
        }

        internal SessionKeyHolder SessionKeyHolder()
        {
            return this.sessionKeyHolder;
        }

        internal LtKeyHolder LtKeyHolder()
        {
            return this.ltKeyHolder;
        }
        internal OtKeyHolder OtKeyHolder()
        {
            return this.otKeyHolder;
        }
    }
}
