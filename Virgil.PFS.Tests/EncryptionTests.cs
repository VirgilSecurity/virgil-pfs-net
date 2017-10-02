using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Virgil.PFS.Session;
using Virgil.SDK.Cryptography;
using Virgil.SDK.Exceptions;
using Virgil.SDK.Storage;

namespace Virgil.PFS.Tests
{
    class EncryptionTests
    {
        [Test]
        public async Task Decrypt_Should_ReturnOriginalText_ForInitialMessage()
        {
            var crypto = new VirgilCrypto();

            var virgil = IntegrationHelper.GetVirgilApi();
            var aliceKeys = virgil.Keys.Generate();
            var bobKeys = virgil.Keys.Generate();

            var aliceCard = await IntegrationHelper.CreateCard("Alice" + Guid.NewGuid(), aliceKeys);
            var bobCard = await IntegrationHelper.CreateCard("Bob" + Guid.NewGuid(), bobKeys);

            var secureChatParamsForAlice = new SecureChatPreferences(
                crypto,
                aliceCard.CardModel,
                aliceKeys.PrivateKey,
                IntegrationHelper.GetServiceInfo()
                );

            var secureChatParamsForBob = new SecureChatPreferences(
                crypto,
                bobCard.CardModel,
                bobKeys.PrivateKey,
                IntegrationHelper.GetServiceInfo());

            var secureChatForAlice = new SecureChat(secureChatParamsForAlice);
            var secureChatForBob = new SecureChat(secureChatParamsForBob);

            await secureChatForBob.RotateKeysAsync(1);
            await secureChatForAlice.RotateKeysAsync(1);

            var aliceSession = await secureChatForAlice.StartNewSessionWithAsync(bobCard.CardModel);

            var originalText = "Hi Bob!";
            var encryptedMessage = aliceSession.Encrypt(originalText);

            var bobSession = await secureChatForBob.LoadUpSession(aliceCard.CardModel, encryptedMessage);
            var decryptedMessage = bobSession.Decrypt(encryptedMessage);

            Assert.AreEqual(originalText, decryptedMessage);

            var activeBobSession = secureChatForBob.ActiveSession(aliceCard.Id);
            Assert.AreEqual(originalText, activeBobSession.Decrypt(encryptedMessage));

            secureChatForAlice.GentleReset();
            secureChatForBob.GentleReset();
            await IntegrationHelper.RevokeCard(aliceCard);
            await IntegrationHelper.RevokeCard(bobCard);

        }

        [Test]
        public async Task Decrypt_Should_ReturnOriginalText_ForSecondMessage()
        {
            var crypto = new VirgilCrypto();
            var virgil = IntegrationHelper.GetVirgilApi();

            var aliceKey = virgil.Keys.Generate();
            var bobKey = virgil.Keys.Generate();
            var aliceCard = await IntegrationHelper.CreateCard("Alice" + Guid.NewGuid(), aliceKey);
            var bobCard = await IntegrationHelper.CreateCard("Bob" + Guid.NewGuid(), bobKey);

            var secureChatParamsForAlice = new SecureChatPreferences(
                crypto,
                aliceCard.CardModel,
                aliceKey.PrivateKey,
                IntegrationHelper.GetServiceInfo()
                );

            var secureChatParamsForBob = new SecureChatPreferences(
                crypto,
                bobCard.CardModel,
                bobKey.PrivateKey,
                IntegrationHelper.GetServiceInfo());

            var sessionStorage = new DefaultUserDataStorage(bobCard.Id);
            var sessionHelper = new SessionStorageManager(sessionStorage);
            var keyStorageManger = new KeyStorageManger(crypto, bobCard.Id, secureChatParamsForBob.LtPrivateKeyLifeDays);

            var secureChatForAlice = new SecureChat(secureChatParamsForAlice);
            var secureChatForBob = new SecureChat(secureChatParamsForBob);

            await secureChatForBob.RotateKeysAsync(1);
            await secureChatForAlice.RotateKeysAsync(1);

            var aliceSession = await secureChatForAlice.StartNewSessionWithAsync(bobCard.CardModel);

            var originalText = "Hi Bob!";
            var encryptedMessage = aliceSession.Encrypt(originalText);

            var bobSession = await secureChatForBob.LoadUpSession(aliceCard.CardModel, encryptedMessage);
            var decryptedMessage = bobSession.Decrypt(encryptedMessage);

            Assert.AreEqual(originalText, decryptedMessage);

            originalText = "Hi Alice!";
            encryptedMessage = bobSession.Encrypt(originalText);

            Assert.AreEqual(originalText, aliceSession.Decrypt(encryptedMessage));

            var activeAliceSession = secureChatForAlice.ActiveSession(bobCard.Id);
            originalText = "Are you here?";
            encryptedMessage = activeAliceSession.Encrypt(originalText);

            Assert.AreEqual(originalText, bobSession.Decrypt(encryptedMessage));

            secureChatForAlice.GentleReset();
            secureChatForBob.GentleReset();
            await IntegrationHelper.RevokeCard(aliceCard);
            await IntegrationHelper.RevokeCard(bobCard);

        }
    }
}
