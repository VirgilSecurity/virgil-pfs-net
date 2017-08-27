using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using Virgil.SDK.Cryptography;
using Virgil.SDK.Client;
using Virgil.PFS;

namespace Virgil.PFS.Tests
{
    [TestFixture]
    public class SecureChatTests
    {
        [Test]
        public void StartUpSession_Should_Throw_Exception_If_Session_Exists()
        {
           /* var crypto = Substitute.For<ICrypto>();
            var alicePrivateKey = Substitute.For<IPrivateKey>();
            var alicePublicKey = Substitute.For<IPublicKey>();
            var aliceCard = new CardModel();
            var secureChatParamsForAlice = new SecureChatParams(
                crypto,
                aliceCard,
                alicePrivateKey,
               new ServiceInfo() { AccessToken = appAccessToken, Address = "https://pfs-stg.virgilsecurity.com" });*/
        }

        [Test]
        public void StartUpSession_Should_Create_Session_If_Expired_Session_Exists()
        {

        }

        // if session does not exist
        [Test]
        public void StartUpSession_Should_Throw_Exception_If_Credentials_Are_Empty()
        {

        }

        [Test]
        public void StartUpSession_Should_Throw_Exception_If_Credentials_Doesnot_Have_Identity_Sign()
        {

        }

    }
}
