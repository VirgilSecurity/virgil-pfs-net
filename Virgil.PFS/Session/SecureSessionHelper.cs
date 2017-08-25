namespace Virgil.PFS
{
    using System;
    using System.Collections.Generic;
    using Virgil.PFS.Client;
    using Virgil.PFS.Exceptions;

    public class SecureSessionHelper
    {
        private string ownerCardId;
        private ISessionStateHolder sessionStateHolder;

        public SecureSessionHelper(string cardId)
        {
            this.ownerCardId = cardId;

            this.sessionStateHolder = new SessionStateHolder(cardId);
        }

        public SessionState GetSessionState(string cardId)
        {
            var sessionStateJson = this.sessionStateHolder.Load(cardId);
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
            return this.sessionStateHolder.LoadAllNames();
        }

        public void DeleteSessionState(string cardId)
        {
            this.sessionStateHolder.Delete(cardId);
        }

        public bool ExistSessionState(string cardId)
        {
            return this.sessionStateHolder.Exists(cardId);
        }

        public void SaveSessionState(SessionState sessionState, string cardId)
        {
            this.sessionStateHolder.Save(JsonSerializer.Serialize(sessionState), cardId);
        }

        public struct SessionInfo
        {
            public string CardId;
            public SessionState SessionState;
        }

    }
}
