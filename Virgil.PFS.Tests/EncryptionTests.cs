using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Virgil.PFS.Session;
using Virgil.PFS.Session.Default;
using Virgil.SDK.Cryptography;

namespace Virgil.PFS.Tests
{
    class EncryptionTests
    {
        [Test]
        public async Task Decrypt_Should_ReturnOriginalText_ForInitialMessage()
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

            var sessionStorage = new DefaultUserDataStorage();

            var secureChatForAlice = new SecureChat(secureChatParamsForAlice);
            var secureChatForBob = new SecureChat(secureChatParamsForBob);

            await secureChatForBob.RotateKeysAsync(1);
            await secureChatForAlice.RotateKeysAsync(1);

            var aliceSession = await secureChatForAlice.StartNewSessionWithAsync(bobCard);

            var originalText = "Hi Bob!";
            var encryptedMessage = aliceSession.Encrypt(originalText);

            var bobSession = await secureChatForBob.LoadUpSession(aliceCard, encryptedMessage);
            var decryptedMessage = bobSession.Decrypt(encryptedMessage);

            Assert.AreEqual(originalText, decryptedMessage);

            var activeBobSession = secureChatForBob.ActiveSession(aliceCard.Id);
            Assert.AreEqual(originalText, activeBobSession.Decrypt(encryptedMessage));

            secureChatForAlice.GentleReset();
            secureChatForBob.GentleReset();
            await IntegrationHelper.RevokeCard(aliceCard.Id);
            await IntegrationHelper.RevokeCard(bobCard.Id);

        }

        [Test]
        public async Task Decrypt_Should_ReturnOriginalText_ForSecondMessage()
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

            var sessionStorage = new DefaultUserDataStorage();
            var sessionHelper = new SessionStorageManager(bobCard.Id, sessionStorage);
            var keyStorageManger = new KeyStorageManger(crypto, bobCard.Id, secureChatParamsForBob.LtPrivateKeyLifeDays);

            var secureChatForAlice = new SecureChat(secureChatParamsForAlice);
            var secureChatForBob = new SecureChat(secureChatParamsForBob);

            await secureChatForBob.RotateKeysAsync(1);
            await secureChatForAlice.RotateKeysAsync(1);

            var aliceSession = await secureChatForAlice.StartNewSessionWithAsync(bobCard);

            var originalText = "Hi Bob!";
            var encryptedMessage = aliceSession.Encrypt(originalText);

            var bobSession = await secureChatForBob.LoadUpSession(aliceCard, encryptedMessage);
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
            await IntegrationHelper.RevokeCard(aliceCard.Id);
            await IntegrationHelper.RevokeCard(bobCard.Id);

        }
    }
}
