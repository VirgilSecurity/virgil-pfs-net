using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Virgil.SDK.Client;
using Virgil.SDK.Common;
using Virgil.SDK.Cryptography;
using Virgil.PFS.Exceptions;
using Virgil.PFS;
using Virgil.PFS.Session;
using Virgil.PFS.Session.Default;

namespace Virgil.PFS.Tests
{
    class StartUpSessionTests
    {
        [Test]
        public async Task StartUpSession_Should_ThrowException_If_Session_Exists()
        {
            var crypto = new VirgilCrypto();

            // bob's side
            var bobKeys = crypto.GenerateKeys();
            var bobCard = await IntegrationHelper.CreateCard("Bob" + Guid.NewGuid(), bobKeys);
            var secureChatParamsForBob = new SecureChatParams(
                crypto,
                bobCard,
                bobKeys.PrivateKey,
                IntegrationHelper.GetServiceInfo());
            var secureChatForBob = new SecureChat(secureChatParamsForBob);
            await secureChatForBob.RotateKeysAsync(1);

            //alice's side
            var aliceKeys = crypto.GenerateKeys();
            var aliceCard = await IntegrationHelper.CreateCard("Alice" + Guid.NewGuid(), aliceKeys);
            var secureChatParamsForAlice = new SecureChatParams(
                crypto,
                aliceCard,
                aliceKeys.PrivateKey,
                IntegrationHelper.GetServiceInfo());

            var secureChatForAlice = new SecureChat(secureChatParamsForAlice);
            var sessionA = await secureChatForAlice.StartNewSessionWithAsync(bobCard);
            sessionA.Encrypt("hi bob!");
            Assert.ThrowsAsync<SecureSessionException>(async () => await secureChatForAlice.StartNewSessionWithAsync(bobCard));

            await IntegrationHelper.RevokeCard(aliceCard.Id);
            await IntegrationHelper.RevokeCard(bobCard.Id);


        }

        [Test]
        public async Task Decrypt_Should_ThrowException_If_StartUppedSessionDidntEncryptBefore()
        {
            var crypto = new VirgilCrypto();

            // bob's side
            var bobKeys = crypto.GenerateKeys();
            var bobCard = await IntegrationHelper.CreateCard("Bob" + Guid.NewGuid(), bobKeys);
            var secureChatParamsForBob = new SecureChatParams(
                crypto,
                bobCard,
                bobKeys.PrivateKey,
                IntegrationHelper.GetServiceInfo());
            var secureChatForBob = new SecureChat(secureChatParamsForBob);
            await secureChatForBob.RotateKeysAsync(1);

            //alice's side
            var aliceKeys = crypto.GenerateKeys();
            var aliceCard = await IntegrationHelper.CreateCard("Alice" + Guid.NewGuid(), aliceKeys);
            var secureChatParamsForAlice = new SecureChatParams(
                crypto,
                aliceCard,
                aliceKeys.PrivateKey,
                IntegrationHelper.GetServiceInfo());

            var secureChatForAlice = new SecureChat(secureChatParamsForAlice);
            var sessionA = await secureChatForAlice.StartNewSessionWithAsync(bobCard);
            Assert.Throws<SecureSessionException>(() => sessionA.Decrypt("some_encrypted_message"));

            await IntegrationHelper.RevokeCard(aliceCard.Id);
            await IntegrationHelper.RevokeCard(bobCard.Id);
        }

        [Test]
        public async Task StartUpSession_Should_ThrowException_If_InterlocutorDoesntHaveCredentials()
        {
            var crypto = new VirgilCrypto();

            var aliceKeys = crypto.GenerateKeys();
            var bobKeys = crypto.GenerateKeys();

            var aliceCard = await IntegrationHelper.CreateCard("Alice" + Guid.NewGuid(), aliceKeys);
            var bobCard = await IntegrationHelper.CreateCard("Bob" + Guid.NewGuid(), bobKeys);

            var secureChatParamsForAlice = new SecureChatParams(
                crypto,
                aliceCard,
                aliceKeys.PrivateKey,
                IntegrationHelper.GetServiceInfo());

            var secureChatForAlice = new SecureChat(secureChatParamsForAlice);
            //var keyHelper = new SecureChatKeyHelper(crypto, aliceCard.Id, secureChatParamsForAlice.LtPrivateKeyLifeDays);

            Assert.ThrowsAsync<CredentialsException>(async () => await secureChatForAlice.StartNewSessionWithAsync(bobCard));

            await IntegrationHelper.RevokeCard(aliceCard.Id);
            await IntegrationHelper.RevokeCard(bobCard.Id);
        }

        [Test]
        public async Task StartUpSession_Should_CreateSession_If_ExpiredSessionExists()
        {
            var crypto = new VirgilCrypto();

            var aliceKeys = crypto.GenerateKeys();
            var bobKeys = crypto.GenerateKeys();

            var aliceCard = await IntegrationHelper.CreateCard("Alice" + Guid.NewGuid(), aliceKeys);
            var bobCard = await IntegrationHelper.CreateCard("Bob" + Guid.NewGuid(), bobKeys);

            var secureChatParamsForAlice = new SecureChatParams(
                crypto,
                aliceCard,
                aliceKeys.PrivateKey,
                IntegrationHelper.GetServiceInfo()
                );

            var secureChatParamsForBob = new SecureChatParams(
                crypto,
                aliceCard,
                aliceKeys.PrivateKey,
                IntegrationHelper.GetServiceInfo());

            var expiredSessionState = new SessionState(
              new byte[] { },
              DateTime.Now.AddDays(-5),
              DateTime.Now.AddDays(-1),
              new byte[] { });

            var sessionStorage = new DefaultUserDataStorage();
            var sessionHelper = new SecureSessionHelper(aliceCard.Id, sessionStorage);

            Assert.IsFalse(sessionHelper.ExistSessionState(bobCard.Id));
            sessionHelper.SaveSessionState(expiredSessionState, bobCard.Id);
            Assert.IsTrue(sessionHelper.ExistSessionState(bobCard.Id));

            var secureChatForAlice = new SecureChat(secureChatParamsForAlice);
            var secureChatForBob = new SecureChat(secureChatParamsForBob);

            await secureChatForBob.RotateKeysAsync(1);
            await secureChatForAlice.RotateKeysAsync(1);

            var session = await secureChatForAlice.StartNewSessionWithAsync(bobCard);
            // Initialize session and save session key, session state
            session.Encrypt("Hi Bob!");

            Assert.IsTrue(sessionHelper.ExistSessionState(bobCard.Id));
            Assert.IsFalse(
                Enumerable.SequenceEqual(
                    sessionHelper.GetSessionState(bobCard.Id).SessionId, 
                    expiredSessionState.SessionId)
            );

            sessionHelper.DeleteSessionState(bobCard.Id);
            await IntegrationHelper.RevokeCard(aliceCard.Id);
            await IntegrationHelper.RevokeCard(bobCard.Id);

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
