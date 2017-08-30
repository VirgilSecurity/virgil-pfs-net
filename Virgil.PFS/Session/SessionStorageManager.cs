namespace Virgil.PFS.Session
{
    using System;
    using System.Collections.Generic;
    using Virgil.PFS.Client;
    using System.Linq;


    internal class SessionStorageManager
    {
        private string ownerCardId;
        private IUserDataStorage sessionStorage;

        public SessionStorageManager(string cardId, IUserDataStorage sessionStorage)
        {
            this.ownerCardId = cardId;

            this.sessionStorage = sessionStorage;
        }
        private string GetSessionPathPrefix()
        {
            return $"{ownerCardId}--";
        }
        private string GetSessionPath(string cardId)
        {
            return this.GetSessionPathPrefix() + cardId;
        }

        public SessionState GetSessionState(string cardId)
        {
            var stateSessionPath = this.GetSessionPath(cardId);
            var sessionStateJson = this.sessionStorage.Load(stateSessionPath);
            return JsonSerializer.Deserialize<SessionState>(sessionStateJson, true);
        }

        public List<SessionInfo> GetAllSessionStates()
        {
            var sessionStates = new List<SessionInfo>();
            foreach (var sessionStateName in this.GetAllSessionStateIds())
            {
                var sessionState = this.GetSessionState(sessionStateName);
                var el = new SessionInfo()
                {
                    CardId = sessionStateName,
                    SessionState = sessionState
                };
                sessionStates.Add(el);
            }
            return sessionStates;

        }


        public string[] GetAllSessionStateIds()
        {
            var cardIds = new List<string>();

            var sessionStatePaths = this.sessionStorage.LoadAllNames();
            var ownerStatePaths = Array.FindAll(
                sessionStatePaths, s => s.Contains(this.GetSessionPathPrefix()));
            foreach(var sessionStatePath in ownerStatePaths)
            {
                string cardId = sessionStatePath.Split(
                    new string[] { this.GetSessionPathPrefix() }, 
                    StringSplitOptions.None).Last();
                cardIds.Add(cardId);
            }

            return cardIds.ToArray();
        }

        public void DeleteSessionState(string cardId)
        {
            this.sessionStorage.Delete(this.GetSessionPath(cardId));
        }

        public bool ExistSessionState(string cardId)
        {
            return this.sessionStorage.Exists(this.GetSessionPath(cardId));
        }

        public void SaveSessionState(SessionState sessionState, string cardId)
        {
            this.sessionStorage.Save(JsonSerializer.Serialize(sessionState), this.GetSessionPath(cardId));
        }

        public struct SessionInfo
        {
            public string CardId;
            public SessionState SessionState;
        }

    }
}
