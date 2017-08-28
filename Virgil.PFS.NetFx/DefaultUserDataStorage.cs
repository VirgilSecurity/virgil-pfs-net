namespace Virgil.PFS.Session.Default
{
    using Exceptions;
    using System;
    using System.IO;
    using System.Linq;
    using System.Text;

    public class DefaultUserDataStorage : IUserDataStorage
    {
        private const string sessionFolder = "Sessions";
        private string folderPath;
        protected string ownerId;


        public DefaultUserDataStorage()
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            this.folderPath = Path.Combine(appData, "VirgilSecurity", "Sessions");
            Directory.CreateDirectory(this.folderPath);
        }

        public string FolderPath()
        {
            return this.folderPath;
        }

        public string Load(string cardId)
        {
            if (!this.Exists(cardId))
            {
                throw new SecureSessionHolderException("Session state is not found.");
            }
            var jsonBytes = File.ReadAllBytes(this.GetSessionStatePath(cardId));
            var sessionStateJson = Encoding.UTF8.GetString(jsonBytes);
            return sessionStateJson;
        }



        public string[] LoadAllNames()
        {
            if (Directory.Exists(this.folderPath))
            {
                return Directory.GetFiles(this.folderPath).Select(f => Path.GetFileName(f)).ToArray();
            }
            else
            {
                return new string[] { };
            }
        }

        public bool Exists(string cardId)
        {
            return File.Exists(this.GetSessionStatePath(cardId));
        }

        private string GetSessionStatePath(string alias)
        {
            return Path.Combine(this.folderPath, alias.ToLower());
        }

        public void Save(string sessionStateJson, string cardId)
        {
            Directory.CreateDirectory(this.folderPath);

            if (this.Exists(cardId))
            {
                throw new SecureSessionHolderException("Secure station already exist");
            }

            var sessionStateBytes = Encoding.UTF8.GetBytes(sessionStateJson);

            var sessionStatePath = this.GetSessionStatePath(cardId);

            File.WriteAllBytes(sessionStatePath, sessionStateBytes);
        }

        public void Delete(string cardId)
        {
            if (!this.Exists(cardId))
            {
                throw new SecureSessionHolderException("Session state is not found.");
            }

            File.Delete(this.GetSessionStatePath(cardId));
        }

    }
}

