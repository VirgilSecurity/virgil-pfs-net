using Virgil.SDK.Cryptography;

namespace Virgil.PFS
{
    public class CardSigner
    {
        public string CardId { get; set; }
        public IPrivateKey PrivateKey { get; set; }
    }
}