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
        public static VirgilApi GetVirgilApi()
        {
            var parameters = new VirgilClientParams(AppAccessToken);

            parameters.SetCardsServiceAddress(ConfigurationManager.AppSettings["virgil:CardsServicesAddress"]);
            parameters.SetReadCardsServiceAddress(ConfigurationManager.AppSettings["virgil:CardsReadServicesAddress"]);
            parameters.SetIdentityServiceAddress(ConfigurationManager.AppSettings["virgil:IdentityServiceAddress"]);

            // To use staging Verifier instead of default verifier
            var cardVerifier = new CardVerifierInfo
            {
                CardId = ConfigurationManager.AppSettings["virgil:ServiceCardId"],
                PublicKeyData = VirgilBuffer.From(ConfigurationManager.AppSettings["virgil:ServicePublicKeyDerBase64"],
                StringEncoding.Base64)
            };
            var virgil = new VirgilApi(new VirgilApiContext
            {
                Credentials = new AppCredentials
                {
                    AppId = AppID,
                    AppKey = VirgilBuffer.From(AppKey),
                    AppKeyPassword = AppKeyPassword
                },
                ClientParams = parameters,
                UseBuiltInVerifiers = false,
                CardVerifiers = new[] { cardVerifier }
            });
            return virgil;
        }

        public static string AppID => ConfigurationManager.AppSettings["virgil:AppID"];
        public static byte[] AppKey => File.ReadAllBytes(ConfigurationManager.AppSettings["virgil:AppKeyPath"]);
        public static string AppKeyPath => ConfigurationManager.AppSettings["virgil:AppKeyPath"];
        public static string AppKeyPassword = ConfigurationManager.AppSettings["virgil:AppKeyPassword"];
        public static string AppAccessToken = ConfigurationManager.AppSettings["virgil:AppAccessToken"];


        public static async Task<VirgilCard> CreateCard(string identity, VirgilKey key)
        {
            var virgil = GetVirgilApi();
            var card = virgil.Cards.Create(identity, key);

            await virgil.Cards.PublishAsync(card);

            return card;
        }

        public static async Task RevokeCard(VirgilCard card)
        {
            var virgil = GetVirgilApi();
            await virgil.Cards.RevokeAsync(card);
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
