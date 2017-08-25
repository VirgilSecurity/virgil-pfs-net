using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Virgil.PFS.KeyUtils
{
    //symmetric key
    public class SessionKey
    {
        public byte[] DecryptionKey { get; set; }

        public byte[] EncryptionKey { get; set; }

    }
}
