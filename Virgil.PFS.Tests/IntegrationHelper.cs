using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Virgil.SDK;
using Virgil.SDK.Client;
using Virgil.SDK.Common;
using Virgil.SDK.Cryptography;

namespace Virgil.PFS.Tests
{
    public class IntegrationHelper
    {
        public static VirgilClient GetVirgilClient()
        {
            var parameters = new VirgilClientParams(AppAccessToken);

            parameters.SetCardsServiceAddress(ConfigurationManager.AppSettings["virgil:CardsServicesAddress"]);
            parameters.SetReadCardsServiceAddress(ConfigurationManager.AppSettings["virgil:CardsReadServicesAddress"]);
            parameters.SetIdentityServiceAddress(ConfigurationManager.AppSettings["virgil:IdentityServiceAddress"]);

            var client = new VirgilClient(parameters);

            return client;
        }

        public static string AppID => ConfigurationManager.AppSettings["virgil:AppID"];
        public static byte[] AppKey => File.ReadAllBytes(ConfigurationManager.AppSettings["virgil:AppKeyPath"]);
        public static string AppKeyPath => ConfigurationManager.AppSettings["virgil:AppKeyPath"];
        public static string AppKeyPassword = ConfigurationManager.AppSettings["virgil:AppKeyPassword"];
        public static string AppAccessToken = ConfigurationManager.AppSettings["virgil:AppAccessToken"];


        public static async Task<CardModel> CreateCard(string identity, KeyPair keyPair)
        {
            var crypto = new VirgilCrypto();
            var client = IntegrationHelper.GetVirgilClient();

            var appKey = crypto.ImportPrivateKey(IntegrationHelper.AppKey, IntegrationHelper.AppKeyPassword);

           
            var exportedPublicKey = crypto.ExportPublicKey(keyPair.PublicKey);

            var aliceIdentity = "alice-" + Guid.NewGuid();

            var request = new PublishCardRequest(identity, "member", exportedPublicKey);

            var requestSigner = new RequestSigner(crypto);

            requestSigner.SelfSign(request, keyPair.PrivateKey);
            requestSigner.AuthoritySign(request, IntegrationHelper.AppID, appKey);
            
            return await client.PublishCardAsync(request);
        }

        public static async Task RevokeCard(string cardId)
        {
            var client = GetVirgilClient();
            var crypto = new VirgilCrypto();
            var requestSigner = new RequestSigner(crypto);

            var appKey = crypto.ImportPrivateKey(AppKey, AppKeyPassword);

            var revokeRequest = new RevokeCardRequest(cardId, RevocationReason.Unspecified);
            requestSigner.AuthoritySign(revokeRequest, AppID, appKey);

            await client.RevokeCardAsync(revokeRequest);
        }

        public static ServiceInfo GetServiceInfo()
        {
           return  new ServiceInfo()
            {
                AccessToken = IntegrationHelper.AppAccessToken,
                Address = "https://pfs-stg.virgilsecurity.com"
            };
        }

       
    }
}
