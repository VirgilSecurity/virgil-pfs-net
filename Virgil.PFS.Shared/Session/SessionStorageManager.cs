namespace Virgil.PFS.Session
{
    using System;
    using System.Collections.Generic;
    using Virgil.PFS.Client;
    using System.Linq;
    using Exceptions;

    internal class SessionStorageManager
    {
        private IUserDataStorage sessionStorage;

        public SessionStorageManager(IUserDataStorage sessionStorage)
        {
            this.sessionStorage = sessionStorage;
        }

        public SessionState GetNewestSessionState(string recipientCardId)
        {
            try
            {
                var sessionState = GetSessionStates(recipientCardId)
                    .OrderByDescending(el => el.ExpiredAt).FirstOrDefault();
                return sessionState;
            }
            catch (Exception)
            {
                throw new SessionStorageException("Session isn't found.");
            }
        }

        public SessionState[] GetSessionStates(string recipientCardId)
        {
            try
            {
                var sessionStatesJson = this.sessionStorage.Load(recipientCardId);
                return JsonSerializer.Deserialize<SessionState[]>(sessionStatesJson, true);
            }
            catch (Exception)
            {
                throw new SessionStorageException("There isn't any session for this recipient.");
            }
        }

        public List<SessionInfo> GetAllSessionStates()
        {
            var sessionStates = new List<SessionInfo>();
            foreach (var recipientId in this.sessionStorage.FileNames())
            {
                var recipientSessionStates = this.GetSessionStates(recipientId);
                
                foreach(var sessionState in recipientSessionStates)
                {
                    var el = new SessionInfo()
                    {
                        CardId = recipientId,
                        SessionState = sessionState
                    };
                    sessionStates.Add(el);
                }
            }
            return sessionStates;
        }


        public void RemoveAllSessionStates()
        {
            foreach (var recipientCardId in this.sessionStorage.FileNames())
            {
                this.RemoveSessionStates(recipientCardId);
            }
        }

        public void RemoveSessionStates(string recipientCardId)
        {
            try
            {
                this.sessionStorage.Delete(recipientCardId);
            }
            catch (Exception)
            {
                throw new SessionStorageException("Session isn't found.");
            }
        }

        public void RemoveSessionState(string recipientCardId, byte[] sessionId)
        {
            if (this.ExistSessionStates(recipientCardId))
            {
                var sessionStates = this.GetSessionStates(recipientCardId);
                var sessionState = sessionStates.FirstOrDefault(el => Enumerable.SequenceEqual(el.SessionId,sessionId));
                if (sessionState != null)
                {
                    var sessionStatesList = sessionStates.ToList();
                    sessionStatesList.Remove(sessionState);
                    if (sessionStatesList.Count > 0)
                    {
                        var sessionStatesJson = JsonSerializer.Serialize(sessionStatesList.ToArray());
                        this.sessionStorage.Update(sessionStatesJson, recipientCardId);
                    }
                    else
                    {
                        this.sessionStorage.Delete(recipientCardId);
                    }

                }
                else
                {
                    throw new SessionStorageException("Session isn't found.");
                }
            }
            else
            {
                throw new SessionStorageException("Session isn't found.");
            }
        }


        public bool ExistSessionState(string recipientCardId, byte[] sessionId)
        {
            if (this.sessionStorage.Exists(recipientCardId))
            {
                var sessionStates = this.GetSessionStates(recipientCardId);
                return (sessionStates.Any(el => Enumerable.SequenceEqual(el.SessionId, sessionId)));
            }
            return false;
        }

        public bool ExistSessionStates(string recipientCardId)
        {
            return this.sessionStorage.Exists(recipientCardId);
        }

        public void SaveSessionState(SessionState sessionState, string recipientCardId)
        {
            if (this.sessionStorage.Exists(recipientCardId))
            {
                var sessionStates = this.GetSessionStates(recipientCardId);
                if (sessionStates.Any(el => Enumerable.SequenceEqual(el.SessionId, sessionState.SessionId)))
                {
                    throw new SessionStorageException("Session already exist");
                }
                var sessionStatesList = sessionStates.ToList();
                sessionStatesList.Add(sessionState);
                var sessionStatesJson = JsonSerializer.Serialize(sessionStatesList.ToArray());
                this.sessionStorage.Update(sessionStatesJson, recipientCardId);
            }
            else
            {
                this.sessionStorage.Save(
                    JsonSerializer.Serialize(new SessionState[] {sessionState}), recipientCardId);
            }
        }

        public struct SessionInfo
        {
            public string CardId;
            public SessionState SessionState;
        }
    }
}
