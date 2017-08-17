using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Virgil.SDK.Cryptography;

namespace Virgil.PFS
{
    internal class LtKeyHolder : KeyHolder
    {
        public LtKeyHolder(ICrypto crypto, string ownerCardId) : base(crypto, ownerCardId)
        {
        }

        public override string GenerateKeyName(string cardId)
        {
            return $"lt.{cardId}";
        }
    }
}
