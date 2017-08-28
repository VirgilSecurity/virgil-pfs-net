namespace Virgil.PFS.Session
{
    using System;
    using System.Collections.Generic;
    using Virgil.PFS.Client;
    using System.Linq;


    public class SecureSessionHelper
    {
        private string ownerCardId;
        private IUserDataStorage sessionStateHolder;

        public SecureSessionHelper(string cardId, IUserDataStorage sessionStorage)
        {
            this.ownerCardId = cardId;

            this.sessionStateHolder = sessionStorage;
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
            var sessionStateJson = this.sessionStateHolder.Load(stateSessionPath);
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

            var sessionStatePaths = this.sessionStateHolder.LoadAllNames();
            foreach(var sessionStatePath in sessionStatePaths)
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
            this.sessionStateHolder.Delete(this.GetSessionPath(cardId));
        }

        public bool ExistSessionState(string cardId)
        {
            return this.sessionStateHolder.Exists(this.GetSessionPath(cardId));
        }

        public void SaveSessionState(SessionState sessionState, string cardId)
        {
            this.sessionStateHolder.Save(JsonSerializer.Serialize(sessionState), this.GetSessionPath(cardId));
        }

        public struct SessionInfo
        {
            public string CardId;
            public SessionState SessionState;
        }

    }
}
