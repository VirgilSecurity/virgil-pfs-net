using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Virgil.SDK.Client;
using Virgil.SDK.Common;
using Virgil.SDK.Cryptography;

namespace Virgil.PFS
{
    public class EphemeralCardValidator : ICardValidator
    {
        private readonly Dictionary<string, IPublicKey> verifiers;
        private readonly ICrypto crypto;


        public EphemeralCardValidator(ICrypto crypto)
        {
            this.crypto = crypto;
            this.verifiers = new Dictionary<string, IPublicKey>();
        }

        public void AddVerifier(string verifierCardId, byte[] verifierPublicKey)
        {
            if (string.IsNullOrWhiteSpace(verifierCardId))
                throw new ArgumentException($"Wrong argument {nameof(verifierCardId)}");

            if (verifierPublicKey == null)
                throw new ArgumentNullException(nameof(verifierPublicKey));

            var publicKey = this.crypto.ImportPublicKey(verifierPublicKey);
            this.verifiers.Add(verifierCardId, publicKey);
        }

        public bool Validate(CardModel card)
        {
            var allVerifiers = this.verifiers.ToDictionary(it => it.Key, it => it.Value);
            var fingerprint = this.crypto.CalculateFingerprint(card.Snapshot);

            foreach (var verifier in allVerifiers)
            {
                if (!card.Meta.Signatures.ContainsKey(verifier.Key))
                {
                    return false;
                }

                var isValid = this.crypto.Verify(fingerprint.GetValue(),
                    card.Meta.Signatures[verifier.Key], verifier.Value);

                if (!isValid)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
