using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Virgil.PFS.KeyUtils;

namespace Virgil.PFS.Session
{
    public interface ISession
    {
        string Decrypt(string encryptedMessage);
        string Decrypt(Message msg);
        string Encrypt(string message);
        bool IsInitialized();
        byte[] GetId();
        SessionKey GetKey();
        byte[] GetAdditionalData();
    }
}
