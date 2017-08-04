namespace Virgil.PFS.Client
{
    using Virgil.SDK.Cryptography;

    public class EphemeralCardParams
    {
        public string Identity { get; set; }
        public KeyPair KeyPair { get; set; }
    }
}
