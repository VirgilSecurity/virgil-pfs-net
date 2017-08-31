using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Virgil.SDK.Exceptions;

namespace Virgil.PFS.Exceptions
{
    public class CredentialsException : VirgilException
    {
        public CredentialsException(string message) : base(message)
        {
        }
    }
}
