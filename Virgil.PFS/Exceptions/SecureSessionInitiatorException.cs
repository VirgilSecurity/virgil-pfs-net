using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Virgil.PFS.Exceptions
{
    public class SecureSessionInitiatorException : SecureSessionException
    {
        public SecureSessionInitiatorException(string message) : base(message)
        {
        }
    }
}
