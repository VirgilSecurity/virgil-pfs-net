using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Virgil.PFS.Client;

namespace Virgil.PFS
{
    class MessageHelper
    {
        public static bool IsInitialMessage(string message)
        {
            return (TryExtractInitialMessage(message) != null);
        }

        public static bool IsMessage(string message)
        {
            return (TryExtractMessage(message) != null);
        }

        private static InitialMessage TryExtractInitialMessage(string message)
        {
            InitialMessage msg = null;
            try
            {
                msg = JsonSerializer.Deserialize<InitialMessage>(message, true);
            }
            catch (Exception)
            {
               
            }
            return msg;
        }

        private static Message TryExtractMessage(string message)
        {
            Message msg = null;
            try
            {
                msg = JsonSerializer.Deserialize<Message>(message, true);
            }
            catch (Exception)
            {

            }
            return msg;
        }

        public static InitialMessage ExtractInitialMessage(string message)
        {
            var msg = TryExtractInitialMessage(message);
            if (msg == null)
            {
                throw new Exception("Wrong initial message format."); //todo virgil exception
            }
            return msg;
        }

        public static Message ExtractMessage(string message)
        {
            Message msg = TryExtractMessage(message);
            if (msg == null)
            {
                throw new Exception("Wrong message format."); //todo virgil exception
            }
            return msg;
        }

    }
}
