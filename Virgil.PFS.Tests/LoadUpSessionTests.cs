using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Virgil.PFS.Exceptions;
using Virgil.PFS.KeyUtils;
using Virgil.PFS.Session;
using Virgil.SDK.Cryptography;

namespace Virgil.PFS.Tests
{
    class LoadUpSessionTests
    {
        [Test]
        public async Task LoadUpSession_Should_SaveSessionFromInitialMessage()
        {
            var crypto = new VirgilCrypto();

            var aliceKeys = crypto.GenerateKeys();
            var bobKeys = crypto.GenerateKeys();

            var aliceCard = await IntegrationHelper.CreateCard("Alice" + Guid.NewGuid(), aliceKeys);
            var bobCard = await IntegrationHelper.CreateCard("Bob" + Guid.NewGuid(), bobKeys);

            var secureChatParamsForAlice = new SecureChatPreferences(
                crypto,
                aliceCard,
                aliceKeys.PrivateKey,
                IntegrationHelper.GetServiceInfo()
                );

            var secureChatParamsForBob = new SecureChatPreferences(
                crypto,
                bobCard,
                bobKeys.PrivateKey,
                IntegrationHelper.GetServiceInfo());

            var sessionStorage = new DefaultUserDataStorage(bobCard.Id);
            var sessionHelper = new SessionStorageManager(sessionStorage);
            var keyStorageManger = new KeyStorageManger(crypto, bobCard.Id, secureChatParamsForBob.LtPrivateKeyLifeDays);

            var secureChatForAlice = new SecureChat(secureChatParamsForAlice);
            var secureChatForBob = new SecureChat(secureChatParamsForBob);

            await secureChatForBob.RotateKeysAsync(1);
            await secureChatForAlice.RotateKeysAsync(1);

            // Initialize session and save session key, session state
            var session = await secureChatForAlice.StartNewSessionWithAsync(bobCard);

            var encryptedMessage = session.Encrypt("Hi Bob!");

            var initialMessage = MessageHelper.ExtractInitialMessage(encryptedMessage);

            // we have ot private key
            Assert.IsTrue(keyStorageManger.OtKeyStorage().IsKeyExist(initialMessage.ResponderOtcId));
            Assert.Null(keyStorageManger.OtKeyStorage().LoadKeyInfoByName(initialMessage.ResponderOtcId).ExpiredAt);

            var bobSession = await secureChatForBob.LoadUpSession(aliceCard, encryptedMessage);
            Assert.IsTrue(sessionHelper.ExistSessionState(aliceCard.Id, bobSession.GetId()));

            // should save seesion key
            Assert.IsTrue(keyStorageManger.SessionKeyStorage().IsKeyExist(session.GetId()));

            Assert.IsFalse(keyStorageManger.OtKeyStorage().IsKeyExist(initialMessage.ResponderOtcId));

            Assert.IsTrue(keyStorageManger.SessionKeyStorage().LoadKeyByName(session.GetId()).DecryptionKey.Length > 0);

            secureChatForAlice.GentleReset();
            secureChatForBob.GentleReset();
            await IntegrationHelper.RevokeCard(aliceCard.Id);
            await IntegrationHelper.RevokeCard(bobCard.Id);
        }


        [Test]
        public async Task LoadUpSession_ShouldRecoverSessionFromSecondMessage()
        {
            var crypto = new VirgilCrypto();

            var aliceKeys = crypto.GenerateKeys();
            var bobKeys = crypto.GenerateKeys();

            var aliceCard = await IntegrationHelper.CreateCard("Alice" + Guid.NewGuid(), aliceKeys);
            var bobCard = await IntegrationHelper.CreateCard("Bob" + Guid.NewGuid(), bobKeys);

            var secureChatParamsForAlice = new SecureChatPreferences(
                crypto,
                aliceCard,
                aliceKeys.PrivateKey,
                IntegrationHelper.GetServiceInfo()
                );

            var secureChatParamsForBob = new SecureChatPreferences(
                crypto,
                bobCard,
                bobKeys.PrivateKey,
                IntegrationHelper.GetServiceInfo());

            var sessionStorage = new DefaultUserDataStorage(bobCard.Id);
            var sessionHelper = new SessionStorageManager(sessionStorage);
            var keyStorageManger = new KeyStorageManger(crypto, bobCard.Id, secureChatParamsForBob.LtPrivateKeyLifeDays);

            var secureChatForAlice = new SecureChat(secureChatParamsForAlice);
            var secureChatForBob = new SecureChat(secureChatParamsForBob);

            await secureChatForBob.RotateKeysAsync(1);
            await secureChatForAlice.RotateKeysAsync(1);

            var session = await secureChatForAlice.StartNewSessionWithAsync(bobCard);

            var encryptedMessage = session.Encrypt("Hi Bob!");
            var secondEncryptedMessage = session.Encrypt("How are you?");
            var initialMessage = MessageHelper.ExtractInitialMessage(encryptedMessage);

            await secureChatForBob.LoadUpSession(aliceCard, encryptedMessage);

            var bobSession = secureChatForBob.LoadUpSession(aliceCard, secondEncryptedMessage);
            Assert.NotNull(bobSession);

            secureChatForAlice.GentleReset();
            secureChatForBob.GentleReset();
            await IntegrationHelper.RevokeCard(aliceCard.Id);
            await IntegrationHelper.RevokeCard(bobCard.Id);
        }

        [Test]
        public async Task LoadUpSession_Should_ThrowException_If_DidntGetFirstMessage()
        {
            var crypto = new VirgilCrypto();

            var aliceKeys = crypto.GenerateKeys();
            var bobKeys = crypto.GenerateKeys();

            var aliceCard = await IntegrationHelper.CreateCard("Alice" + Guid.NewGuid(), aliceKeys);
            var bobCard = await IntegrationHelper.CreateCard("Bob" + Guid.NewGuid(), bobKeys);

            var secureChatParamsForAlice = new SecureChatPreferences(
                crypto,
                aliceCard,
                aliceKeys.PrivateKey,
                IntegrationHelper.GetServiceInfo()
                );

            var secureChatParamsForBob = new SecureChatPreferences(
                crypto,
                bobCard,
                bobKeys.PrivateKey,
                IntegrationHelper.GetServiceInfo());

            var secureChatForAlice = new SecureChat(secureChatParamsForAlice);
            var secureChatForBob = new SecureChat(secureChatParamsForBob);

            await secureChatForBob.RotateKeysAsync(1);
            await secureChatForAlice.RotateKeysAsync(1);

            var session = await secureChatForAlice.StartNewSessionWithAsync(bobCard);
            var encryptedMessage = session.Encrypt("Hi Bob!");
            var secondEncryptedMessage = session.Encrypt("How are you?");
            var initialMessage = MessageHelper.ExtractInitialMessage(encryptedMessage);

            Assert.ThrowsAsync<SessionStorageException>(
                async () => await secureChatForBob.LoadUpSession(aliceCard, secondEncryptedMessage)
                );

            secureChatForAlice.GentleReset();
            secureChatForBob.GentleReset();
            await IntegrationHelper.RevokeCard(aliceCard.Id);
            await IntegrationHelper.RevokeCard(bobCard.Id);
        }
    }
}
