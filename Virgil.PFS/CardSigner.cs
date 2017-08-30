using Virgil.SDK.Cryptography;

namespace Virgil.PFS
{
    internal class CardSigner
    {
        public string CardId { get; set; }
        public IPrivateKey PrivateKey { get; set; }
    }
}