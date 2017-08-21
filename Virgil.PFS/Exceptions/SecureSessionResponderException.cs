using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Virgil.PFS.Exceptions
{
    public class SecureSessionResponderException : SecureSessionException
    {
        public SecureSessionResponderException(string message) : base(message)
        {
        }
    }
}
