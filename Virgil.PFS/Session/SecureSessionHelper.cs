using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sample;
using Virgil.PFS;
using Virgil.PFS.Client;

namespace Virgil.PFS
{
    internal class SecureSessionHelper
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
            return this.TryDeserializeSessionState(sessionStateJson);
        }

        public Result GetAllSessionStates()
          {
              var sessionStateNames = this.sessionStateHolder.LoadAllNames();
              List<InitiatorStruct> initiatorSessionStates = new List<InitiatorStruct>();
              List<ResponderStruct> responderSessionStates = new List<ResponderStruct>();

              foreach (var sessionStateName in sessionStateNames)
              {
                  var sessionState = this.TryDeserializeSessionState(sessionStateName);
                  if (sessionState.GetType() == typeof(InitiatorSessionState))
                  {
                    var el = new InitiatorStruct()
                    {
                        CardId = sessionStateName,
                        SessionState = (InitiatorSessionState)sessionState
                    };
                      initiatorSessionStates.Add(el);
                  }
                  else
                  {
                      var el = new ResponderStruct()
                      {
                          CardId = sessionStateName,
                          SessionState = (ResponderSessionState)sessionState
                      };
                    responderSessionStates.Add(el);
                  }
              }
              return new Result()
              {
                  Initiators = initiatorSessionStates,
                  Responders = responderSessionStates
              };

          }

        public List<ResponderSessionState> GetAllResponderSessionStates()
        {
            var sessionStateJsons = this.sessionStateHolder.LoadAll();
            List<ResponderSessionState> responderSessionStates = new List<ResponderSessionState>();

            foreach (var sessionStateJson in sessionStateJsons)
            {
                var sessionState = this.TryDeserializeSessionState(sessionStateJson);
                if (sessionState.GetType() == typeof(ResponderSessionState))
                {
                    responderSessionStates.Add((ResponderSessionState)sessionState);
                }
            }
            return responderSessionStates;
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

        public void DeleteAllSessionStates()
        {
            this.sessionStateHolder.DeleteAll();
        }

        private SessionState TryDeserializeSessionState(string data)
        {
            SessionState sessionState = null;

            try
            {
                sessionState = JsonSerializer.Deserialize<InitiatorSessionState>(data);
                return sessionState;
            }
            catch (Exception)
            {
                try
                {
                    sessionState = JsonSerializer.Deserialize<ResponderSessionState>(data);
                    return sessionState;
                }
                catch (Exception)
                {
                    throw new Exception("Corrupted saved session state"); //todo virgil exception
                }
            }
        }
    }

    internal struct Result
    {
        public List<InitiatorStruct> Initiators;
        public List<ResponderStruct> Responders;
    }

    internal struct InitiatorStruct
    {
        public string CardId;
        public InitiatorSessionState SessionState;
    }

    internal struct ResponderStruct
    {
        public string CardId;
        public ResponderSessionState SessionState;
    }

}
