using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Virgil.PFS.Client;
using Virgil.PFS.Exceptions;
using Virgil.SDK.Client;
using Virgil.SDK.Common;
using Virgil.SDK.Cryptography;
using Virgil.SDK.Storage;

namespace Virgil.PFS
{
    internal class EphemeralCardManager
    {
        private readonly EphemeralCardsClient client;
        private readonly ICrypto crypto;
        private readonly EphemeralRequestFactory factory;
        private SecureChatKeyHelper keyHelper;


        public EphemeralCardManager(ICrypto crypto, SecureChatKeyHelper keyHelper, ServiceInfo serviceInfo)
        {
            this.crypto = crypto;
            this.factory = new EphemeralRequestFactory(crypto);
            this.client = new EphemeralCardsClient(serviceInfo.AccessToken, serviceInfo.Address);
            this.keyHelper = keyHelper;

        }

        public async Task BootstrapCardsSet(CardModel identityCard, IPrivateKey identityKey, int desireNumberOfCards)
        {
            EphemeralCardParams longTermCardParams = null;
            if (!this.keyHelper.LtKeyHolder().HasKeys())
            {
                var longTermKeys = crypto.GenerateKeys();
                var longTermCardId = this.GetCardId(identityCard.SnapshotModel.Identity, longTermKeys.PublicKey);
                this.keyHelper.LtKeyHolder().SaveKeyByName(longTermKeys.PrivateKey, longTermCardId);

                longTermCardParams = new EphemeralCardParams()
                {
                    Identity = identityCard.SnapshotModel.Identity,
                    PublicKey = longTermKeys.PublicKey
                };
            }
            //get otcard size
            var otCardsCount = await this.client.GetOTCardsCount(identityCard.Id);
            List<EphemeralCardParams> oneTimeCardParamsList = new List<EphemeralCardParams>();

            var cardSigner = new CardSigner() { CardId = identityCard.Id, PrivateKey = identityKey };

            if (desireNumberOfCards > otCardsCount.Active)
            {
                for (var i = 0; i <= (desireNumberOfCards - otCardsCount.Active); i++)
                {
                    var oneTimeKeyPair = crypto.GenerateKeys();
                    var oneTimeCardId = this.GetCardId(identityCard.SnapshotModel.Identity, oneTimeKeyPair.PublicKey);

                    this.keyHelper.OtKeyHolder().SaveKeyByName(oneTimeKeyPair.PrivateKey, oneTimeCardId);

                    var oneTimeCardParams = new EphemeralCardParams()
                    {
                        Identity = identityCard.SnapshotModel.Identity,
                        PublicKey = oneTimeKeyPair.PublicKey
                    };
                    oneTimeCardParamsList.Add(oneTimeCardParams);
                }

            }

            if (longTermCardParams != null)
            {
                var createCardsRequest = factory.CreateEphemeralCardsRequest(
                    longTermCardParams,
                    oneTimeCardParamsList.ToArray(),
                    new CardSigner[] {cardSigner}
                );

                await this.client.CreateRecipientAsync(identityCard.Id, createCardsRequest);
            }
            else
            {
                if (oneTimeCardParamsList.Count > 0)
                {
                    var createCardsRequest = factory.CreateEphemeralCardsRequest(
                        null,
                        oneTimeCardParamsList.ToArray(),
                        new CardSigner[] { cardSigner }
                    );
                    await this.client.CreateOTCardsAsync(identityCard.Id, createCardsRequest);
                }
            }

        }

        public async Task<CredentialsModel> GetCredentialsByIdentityCard(CardModel identityCard)
        {
            var credentials = (await this.client.SearchCredentialsByIds(new String[] { identityCard.Id })).FirstOrDefault();
           
            var validator = new EphemeralCardValidator(this.crypto);
            validator.AddVerifier(identityCard.Id, identityCard.SnapshotModel.PublicKeyData);

            if (credentials.LTCard == null && credentials.OTCard == null)
            {
                throw new CredentialsException("Error obtaining recipient cards set. Empty set.");
            }

            if (!validator.Validate(credentials.LTCard) ||
                (credentials.OTCard != null && !validator.Validate(credentials.OTCard)))
            {
                throw new CredentialsException("One of responder ephemeral card validation failed.");
            }

            return credentials;
        }

        public async Task<string[]> ValidateOtCards(string identityCardId, IEnumerable<string> otCardIds)
        {
            return (await this.client.ValidateOtCards(identityCardId, otCardIds)).ExhaustedOtCardsIds;
        }

        private string GetCardId(string identity, IPublicKey publicKey)
        {
            var snapshotModel = new PublishCardSnapshotModel
            {
                Identity = identity,
                IdentityType = "member",
                PublicKeyData = crypto.ExportPublicKey(publicKey),
            };

            var snapshotModelJson = JsonSerializer.Serialize(snapshotModel);
            var takenSnapshot = Encoding.UTF8.GetBytes(snapshotModelJson);


            var snapshotFingerprint = this.crypto.CalculateFingerprint(takenSnapshot);
            return snapshotFingerprint.ToHEX();
        }
    }
}
