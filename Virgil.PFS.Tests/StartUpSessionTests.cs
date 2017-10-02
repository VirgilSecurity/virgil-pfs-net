﻿using System;
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
using Virgil.PFS.KeyUtils;
using Virgil.PFS.Session;
using Virgil.SDK;

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
            var secureChatParamsForBob = new SecureChatPreferences(
                crypto,
                bobCard,
                bobKeys.PrivateKey,
                IntegrationHelper.GetServiceInfo());
            var secureChatForBob = new SecureChat(secureChatParamsForBob);
            await secureChatForBob.RotateKeysAsync(1);

            //alice's side
            var aliceKeys = crypto.GenerateKeys();
            var aliceCard = await IntegrationHelper.CreateCard("Alice" + Guid.NewGuid(), aliceKeys);
            var secureChatParamsForAlice = new SecureChatPreferences(
                crypto,
                aliceCard,
                aliceKeys.PrivateKey,
                IntegrationHelper.GetServiceInfo());

            var secureChatForAlice = new SecureChat(secureChatParamsForAlice);
            var sessionA = await secureChatForAlice.StartNewSessionWithAsync(bobCard);
            sessionA.Encrypt("hi bob!");
            Assert.ThrowsAsync<SecureSessionException>(async () => await secureChatForAlice.StartNewSessionWithAsync(bobCard));

            secureChatForAlice.GentleReset();
            secureChatForBob.GentleReset();
            await IntegrationHelper.RevokeCard(aliceCard.Id);
            await IntegrationHelper.RevokeCard(bobCard.Id);


        }

      
        [Test]
        public async Task StartUpSession_Should_ThrowException_If_ResponderDoesntHaveCredentials()
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
                IntegrationHelper.GetServiceInfo());

            var secureChatForAlice = new SecureChat(secureChatParamsForAlice);

            Assert.ThrowsAsync<CredentialsException>(async () => await secureChatForAlice.StartNewSessionWithAsync(bobCard));

            secureChatForAlice.GentleReset();
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

            var expiredSessionState = new SessionState(
              new byte[1],
              DateTime.Now.AddDays(-5),
              DateTime.Now.AddDays(-1),
              new byte[1]);

            var sessionStorage = new DefaultUserDataStorage(aliceCard.Id);
            var sessionHelper = new SessionStorageManager(sessionStorage);
            var keyStorageManger = new KeyStorageManger(crypto, aliceCard.Id, secureChatParamsForAlice.LtPrivateKeyLifeDays);
            Assert.IsFalse(sessionHelper.ExistSessionStates(bobCard.Id));
           

            var secureChatForAlice = new SecureChat(secureChatParamsForAlice);
            var secureChatForBob = new SecureChat(secureChatParamsForBob);

            await secureChatForBob.RotateKeysAsync(1);
            await secureChatForAlice.RotateKeysAsync(1);

            sessionHelper.SaveSessionState(expiredSessionState, bobCard.Id);
            var expiredSessionKey = new SessionKey()
            {
                DecryptionKey = new byte[1],
                EncryptionKey = new byte[1]
            };

            keyStorageManger.SessionKeyStorage().SaveKeyByName(expiredSessionKey, expiredSessionState.SessionId);
            Assert.IsTrue(sessionHelper.ExistSessionState(bobCard.Id, expiredSessionState.SessionId));

            Assert.IsTrue(keyStorageManger.SessionKeyStorage().IsKeyExist(expiredSessionState.SessionId));
            
            // Initialize session and save session key, session state
            var session = await secureChatForAlice.StartNewSessionWithAsync(bobCard);
            session.Encrypt("Hi Bob!");

            Assert.IsTrue(sessionHelper.ExistSessionState(bobCard.Id, session.GetId()));
            Assert.IsFalse(
                Enumerable.SequenceEqual(
                    sessionHelper.GetNewestSessionState(bobCard.Id).SessionId, 
                    expiredSessionState.SessionId)
            );

            Assert.IsTrue(keyStorageManger.SessionKeyStorage()
                .LoadKeyByName(session.GetId()).DecryptionKey.Length > 0);
            secureChatForAlice.GentleReset();
            secureChatForBob.GentleReset();
            await IntegrationHelper.RevokeCard(aliceCard.Id);
            await IntegrationHelper.RevokeCard(bobCard.Id);

        }

        [Test]
        public void StartUpSession_Should_Throw_Exception_If_Credentials_Doesnot_Have_Identity_Sign()
        {
            //need mock
        }



    }
}
