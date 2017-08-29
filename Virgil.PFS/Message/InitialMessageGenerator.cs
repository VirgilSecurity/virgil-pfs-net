using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Virgil.PFS
{
    public class InitialMessageGenerator
    {
        public String InitiatorIcId;

        public String ResponderIcId;

        public String ResponderLtcId;

        public String ResponderOtcId;

        public byte[] EphPublicKey;

        public byte[] EphPublicKeySignature;

        internal InitialMessage Generate(Message message)
        {
            return new InitialMessage()
            {
                CipherText = message.CipherText,
                EphPublicKey = this.EphPublicKey,
                InitiatorIcId = this.InitiatorIcId,
                EphPublicKeySignature = this.EphPublicKeySignature,
                ResponderIcId = this.ResponderIcId,
                ResponderLtcId = this.ResponderLtcId,
                ResponderOtcId = this.ResponderOtcId,
                Salt = message.Salt
            };
        }
    }
}
