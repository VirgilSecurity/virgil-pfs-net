using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Virgil.PFS.KeyUtils;
using Virgil.SDK.Cryptography;

namespace Virgil.PFS
{
    internal class KeyStorageManger
    {
        private ICrypto crypto;
        private string ownerCardId;
        private SessionKeyStorage sessionKeyStorage;
        private LtKeyStorage ltKeyStorage;
        private OtKeyStorage otKeyStorage;


        public KeyStorageManger(ICrypto crypto, string ownerCardId, int ltKeyLifeDays)
        {
            this.crypto = crypto;
            this.ownerCardId = ownerCardId;
            this.sessionKeyStorage = new SessionKeyStorage(ownerCardId);
            this.ltKeyStorage = new LtKeyStorage(crypto, ownerCardId, ltKeyLifeDays);
            this.otKeyStorage = new OtKeyStorage(crypto, ownerCardId);
        }

        internal SessionKeyStorage SessionKeyStorage()
        {
            return this.sessionKeyStorage;
        }

        internal LtKeyStorage LtKeyStorage()
        {
            return this.ltKeyStorage;
        }

        internal OtKeyStorage OtKeyStorage()
        {
            return this.otKeyStorage;
        }

        internal void RemoveAllOtLtKeys()
        {
            this.OtKeyStorage().RemoveAllKeys();
            this.LtKeyStorage().RemoveAllKeys();
            this.SessionKeyStorage().RemoveAllKeys();
        } 

    }
}
