using Virgil.PFS.Client;
using Virgil.PFS.Exceptions;
using Virgil.SDK.Storage;

namespace Virgil.PFS
{
    using Session;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using Virgil.PFS;

    public class UserDataSafeStorage : IUserDataStorage
    {
        protected IKeyStorage keyStorage;
        private const string sessionFolder = "Sessions";
        protected string ownerId;


        public UserDataSafeStorage(string ownerCardId)
        {
             this.keyStorage = new DefaultKeyStorage($"{sessionFolder}\\{ownerCardId}", false);
            this.ownerId = ownerCardId;
        }

        public string Load(string cardId)
        {
            if (!this.Exists(cardId))
            {
                throw new SecureSessionHolderException("Session state is not found.");
            }
            var entry = this.keyStorage.Load(cardId);
            return Encoding.UTF8.GetString(entry.Value, 0, entry.Value.Count());
        }

        public string[] LoadAll()
        {
            var cardIds = this.keyStorage.Names();
            List<string> sessionStates = new List<string>();

            foreach (var cardId in cardIds)
            {
                sessionStates.Add(this.Load(cardId));
            }
            return sessionStates.ToArray();
        }

        public string[] LoadAllNames()
        {
            return this.keyStorage.Names();
        }

        public bool Exists(string cardId)
        {
            return this.keyStorage.Exists(cardId);
        }

        public void Save(string sessionStateJson, string cardId)
        {
            var keyEntry = new KeyEntry
            {
                Name = cardId,
                Value = Encoding.UTF8.GetBytes(sessionStateJson)
            };
            this.keyStorage.Store(keyEntry);
        }

        public void Delete(string cardId)
        {
            if (!this.Exists(cardId))
            {
                throw new SecureSessionHolderException("Session state is not found.");
            }

            this.keyStorage.Delete(cardId);
        }

    }
}

