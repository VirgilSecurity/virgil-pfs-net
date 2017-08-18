﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Virgil.SDK.Cryptography;

namespace Virgil.PFS.KeyUtils
{
    internal class SessionKeyHolder : KeyHolder
    {
        public SessionKeyHolder(ICrypto crypto, string ownerCardId) : base(crypto, ownerCardId)
        {
        }

        protected override string StoragePrefix()
        {
            return ".ss.";
        }
    }
}
