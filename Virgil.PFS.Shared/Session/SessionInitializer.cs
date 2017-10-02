using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Virgil.Crypto.Pfs;
using Virgil.PFS.Exceptions;
using Virgil.SDK;
using Virgil.SDK.Client;
using Virgil.SDK.Cryptography;

namespace Virgil.PFS.Session
{
    internal class SessionInitializer
    {
        private readonly ICrypto crypto;
        private readonly IPrivateKey identityPrivateKey;
        private readonly CardModel identityCard;

        public SessionInitializer(ICrypto crypto, IPrivateKey identityPrivateKey, CardModel identityCard)
        {
            this.crypto = crypto;
            this.identityCard = identityCard;
            this.identityPrivateKey = identityPrivateKey;
        }

        public SecureSession InitializeInitiatorSession(CardModel recipientCard,
            CredentialsModel recipientCredentials,
            byte[] additionalData, DateTime expiredAt)
        {
            var ephemeralKeyPair = crypto.GenerateKeys();
            var ephPrivateKeyData = crypto.ExportPrivateKey(ephemeralKeyPair.PrivateKey);

            var pfsInitiatorPrivateInfo = GetPfsInitiatorPrivateInfo(ephPrivateKeyData);

            var recipientPfsPublicKey = new VirgilPFSPublicKey(recipientCard.SnapshotModel.PublicKeyData);
            var recipientPfsLtPublicKey = new VirgilPFSPublicKey(recipientCredentials.LTCard.SnapshotModel.PublicKeyData);
            VirgilPFSPublicKey recipientPfsOtPublicKey = null;
            if (recipientCredentials.OTCard != null)
            {
                recipientPfsOtPublicKey =
                    new VirgilPFSPublicKey(recipientCredentials.OTCard.SnapshotModel.PublicKeyData);
            }

            var initialMessageGenerator = GetInitialMessageGenerator(
                recipientCard.Id,
                recipientCredentials.LTCard.Id,
                recipientCredentials.OTCard?.Id,
                ephemeralKeyPair.PrivateKey
                );

            var session = new SecureSession(
                recipientPfsPublicKey,
                recipientPfsLtPublicKey,
                recipientPfsOtPublicKey,
                pfsInitiatorPrivateInfo,
                additionalData,
                initialMessageGenerator,
                expiredAt
                );
            return session;
        }

        private VirgilPFSInitiatorPrivateInfo GetPfsInitiatorPrivateInfo(byte[] ephPrivateKeyData)
        {
            var myPrivateKeyData = crypto.ExportPrivateKey(this.identityPrivateKey);
            var pfsPrivateKey = new VirgilPFSPrivateKey(myPrivateKeyData);

            var pfsEphPrivateKey = new VirgilPFSPrivateKey(ephPrivateKeyData);

            return new VirgilPFSInitiatorPrivateInfo(pfsPrivateKey, pfsEphPrivateKey);
        }

        private InitialMessageGenerator GetInitialMessageGenerator(string responderIcId, 
            string responderLtId, 
            string responderOtId,
            IPrivateKey ephPrivateKey)
        {
            var myEphPublicKey = this.crypto.ExtractPublicKey(ephPrivateKey);
            var myEphPublicKeyData = this.crypto.ExportPublicKey(myEphPublicKey);

            var signForEphPublicKey = this.crypto.Sign(myEphPublicKeyData, this.identityPrivateKey);
            var initialMessageGenerator = new InitialMessageGenerator()
            {
                EphPublicKey = myEphPublicKeyData,
                EphPublicKeySignature = signForEphPublicKey,
                InitiatorIcId = this.identityCard.Id,
                ResponderIcId = responderIcId,
                ResponderOtcId = responderOtId,
                ResponderLtcId = responderLtId
            };
            return initialMessageGenerator;
        }


        public SecureSession InitializeResponderSession(byte[] initiatorPublicKeyData, 
            byte[] initiatorEphKey, 
            byte[] additionalData, 
            byte[] myLtPrivateKey,
            byte[] myOtPrivateKeyData, 
            byte[] myPrivateKeyData,
            DateTime expiredAt)
        {
            var pfsLtPrivateKey = new VirgilPFSPrivateKey(myLtPrivateKey);
            var pfsPrivateKey = new VirgilPFSPrivateKey(myPrivateKeyData);

            var initiatorIdentityPublicKey = new VirgilPFSPublicKey(initiatorPublicKeyData);
            var initiatorEphPublicKey = new VirgilPFSPublicKey(initiatorEphKey);
            VirgilPFSPrivateKey pfsOtPrivateKey = null;
            if (myOtPrivateKeyData != null)
            {

                pfsOtPrivateKey = new VirgilPFSPrivateKey(myOtPrivateKeyData);
            }

            return new SecureSession(pfsOtPrivateKey,
               initiatorIdentityPublicKey,
               initiatorEphPublicKey,
               pfsPrivateKey,
               pfsLtPrivateKey,
               additionalData, expiredAt);
        }



    }
}
