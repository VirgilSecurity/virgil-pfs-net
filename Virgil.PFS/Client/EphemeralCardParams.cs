namespace Virgil.PFS.Client
{
    using Virgil.SDK.Cryptography;

    internal class EphemeralCardParams
    {
        public string Identity { get; set; }
        public IPublicKey PublicKey { get; set; }
    }
}
