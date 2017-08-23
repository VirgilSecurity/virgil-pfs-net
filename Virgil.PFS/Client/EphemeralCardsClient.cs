using System.Collections.Generic;

namespace Virgil.PFS
{
    using System;
    using System.Threading.Tasks;
    using Virgil.PFS.Client;
    using Virgil.PFS.Client.Connection;
    using Virgil.SDK.Client;

    public class EphemeralCardsClient
    {
        private readonly string accessToken;
        private readonly Lazy<IConnection> pfsLazyConnection;
        private IConnection PFSConnection =>  this.pfsLazyConnection.Value;
        private Uri pfsSerivceURL;
        private IResponseErrorHandler errorHandler;


        public EphemeralCardsClient() : this(null, null)
        {

        }

        public EphemeralCardsClient(string accessToken, string pfsServiceAddress = "https://pfs.virgilsecurity.com")
        {
            this.accessToken = accessToken;

            if (string.IsNullOrWhiteSpace(pfsServiceAddress))
                throw new ArgumentException(nameof(pfsServiceAddress));

            this.pfsSerivceURL = new Uri(pfsServiceAddress);

            this.pfsLazyConnection = new Lazy<IConnection>(this.InitializePfsConnection);

            this.errorHandler = new PfsServiceResponseErrorHandler();

          }


        public async Task<RecipientModel> CreateRecipientAsync(string identityCardId, EphemeralCardsRequest request)
        {
            var req = HttpRequest.Create(HttpRequestMethod.Put)
                .WithEndpoint($"/v1/recipient/{identityCardId}")
                .WithBody(request);

            var response = await PFSConnection.SendAsync(req).ConfigureAwait(false);
            if (!response.IsSuccessStatuseCode())
            {
                this.errorHandler.ThrowServiceException(response);
            }
            var recipientModel = response.Parse<RecipientModel>();
            return recipientModel;
        }
        
        public async Task<CardModel> CreateLtCardAsync(string identityCardId, EphemeralCardsRequest request)
        {
            var req = HttpRequest.Create(HttpRequestMethod.Post)
             .WithEndpoint($"/v1/recipient/{identityCardId}/actions/push-ltc")
             .WithBody(request.LtcRequestModel);
            var response = await PFSConnection.SendAsync(req).ConfigureAwait(false);
            var cardModel = response.Parse<CardModel>();
            return cardModel;
        }

        public async Task<List<CardModel>> CreateOtCardsAsync(string identityCardId, EphemeralCardsRequest request)
        {
            var req = HttpRequest.Create(HttpRequestMethod.Post)
                .WithEndpoint($"/v1/recipient/{identityCardId}/actions/push-otcs")
                .WithBody(request.OtcRequestModels);

            var response = await PFSConnection.SendAsync(req).ConfigureAwait(false);
            var cardModel = response.Parse<List<CardModel>>();
            return cardModel;
        }

        public async Task<OTCsCountModel> GetOtCardsCount(string identityCardId)
        {
            var req = HttpRequest.Create(HttpRequestMethod.Post)
               .WithEndpoint($"/v1/recipient/{identityCardId}/actions/count-otcs").WithBody("");

            var response = await PFSConnection.SendAsync(req).ConfigureAwait(false);

            var otcsCountModel = response.Parse<OTCsCountModel>();
            return otcsCountModel;


        }
        public async Task<List<CredentialsModel>> SearchCredentialsByIds(string[] identityCardIds)
        {
            var body = new
            {
                identity_cards_ids = identityCardIds
            };
            var req = HttpRequest.Create(HttpRequestMethod.Post)
           .WithEndpoint("/v1/recipient/actions/search-by-ids")
           .WithBody(body);
            var response = await PFSConnection.SendAsync(req).ConfigureAwait(false);

            var credentialsModels = response.Parse<List<CredentialsModel>>();
            return credentialsModels;
        }

        public async Task<ValidateOtcsModel> ValidateOtCards(string identityCardId, IEnumerable<string> otCardIds)
        {
            var body = new
            {
                one_time_cards_ids = otCardIds
            };
            var req = HttpRequest.Create(HttpRequestMethod.Post)
            .WithEndpoint($"/v1/recipient/{identityCardId}/actions/validate-otcs")
            .WithBody(body);
            var response = await PFSConnection.SendAsync(req).ConfigureAwait(false);

           return response.Parse<ValidateOtcsModel>();
        }

        #region
        private IConnection InitializePfsConnection()
        {
            return new ServiceConnection
            {
                BaseURL = this.pfsSerivceURL,
                AccessToken = this.accessToken
        };
        }
        #endregion
    }
}