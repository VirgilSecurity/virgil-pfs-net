using System.Collections.Generic;
using System.Linq;
using Virgil.PFS.Client;
using Virgil.PFS.Client.Models;
using Virgil.SDK.Client;
using Virgil.SDK.Common;
using Virgil.SDK.Cryptography;

namespace Virgil.PFS
{
    public class EphemeralRequestFactory
    {
        private readonly ICrypto crypto;
        private readonly RequestSigner requestSigner;

        public EphemeralRequestFactory(ICrypto crypto)
        {
            this.crypto = crypto;
            this.requestSigner = new RequestSigner(crypto);
        }

        public EphemeralCardsRequest CreateEphemeralCardsRequest(
            EphemeralCardParams ltCard, 
            EphemeralCardParams[] otCards, 
            CardSigner[] signers
            )
        {
            EphemeralCardRequestModel ltcRequestModel = null;
            if (ltCard != null)
                ltcRequestModel = this.CreateLtCardRequestModel(ltCard, signers);

            List<EphemeralCardRequestModel> otcRequestModels = null;
            if (otCards != null)
                otcRequestModels = this.CreateOtCardRequestModels(otCards, signers);

            var request = new EphemeralCardsRequest()
            {
               LtcRequestModel = ltcRequestModel,
               OtcRequestModels = otcRequestModels
            };

            return request;
        }

        private EphemeralCardRequestModel CreateLtCardRequestModel(EphemeralCardParams ltCardParams, CardSigner[] signers)
        {
            var ltcRequest = new PublishCardRequest(ltCardParams.Identity, 
                "member", crypto.ExportPublicKey(ltCardParams.PublicKey));
            
            foreach (var signer in signers)
            {
                requestSigner.AuthoritySign(ltcRequest, signer.CardId, signer.PrivateKey);
            }

            var ltcRequestModel = new EphemeralCardRequestModel()
            {
                ContentSnapshot = ltcRequest.Snapshot,
                Meta = new EphemeralCardRequestMetaModel
                {
                    Signatures = ltcRequest.Signatures.ToDictionary(it => it.Key, it => it.Value)
                }
            };

            return ltcRequestModel;
        }

        private List<EphemeralCardRequestModel> CreateOtCardRequestModels(
            EphemeralCardParams[] otCards, 
            CardSigner[] signers
            )
        {
            var otcRequestModels = new List<EphemeralCardRequestModel>();
            foreach (var otCard in otCards)
            {
                var otcRequest = new PublishCardRequest(otCard.Identity, "member",
                    crypto.ExportPublicKey(otCard.PublicKey));

                foreach (var signer in signers)
                {
                    requestSigner.AuthoritySign(otcRequest, signer.CardId, signer.PrivateKey);
                }
                var otcRequestModel = new EphemeralCardRequestModel()
                {
                    ContentSnapshot = otcRequest.Snapshot,
                    Meta = new EphemeralCardRequestMetaModel
                    {
                        Signatures = otcRequest.Signatures.ToDictionary(it => it.Key, it => it.Value)
                    }
                };
                otcRequestModels.Add(otcRequestModel);
            }
            return otcRequestModels;
        }
    }
}
