using Virgil.PFS.Exceptions;

namespace Virgil.PFS.Session
{
    using Virgil.PFS;
    using System;
    using System.IO;
    using System.Linq;
    using System.Text;

    public class DefaultUserDataStorage : IUserDataStorage
    {
        private const string sessionFolder = "Sessions";
        private string folderPath;
        protected string ownerId;


        public DefaultUserDataStorage(string folderName)
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            this.folderPath = Path.Combine(appData, "VirgilSecurity", "Sessions", folderName);
        }

        public string FolderPath()
        {
            return this.folderPath;
        }

        public string Load(string cardId)
        {
            if (!this.Exists(cardId))
            {
                throw new Exception("Session state is not found.");
            }
            var jsonBytes = File.ReadAllBytes(this.GetFilePath(cardId));
            var dataJson = Encoding.UTF8.GetString(jsonBytes);
            return dataJson;
        }



        public string[] FileNames()
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
            return File.Exists(this.GetFilePath(cardId));
        }

        private string GetFilePath(string alias)
        {
            return Path.Combine(this.folderPath, alias.ToLower());
        }

        public void Save(string dataJson, string cardId)
        {
            Directory.CreateDirectory(this.folderPath);

            if (this.Exists(cardId))
            {
                throw new Exception("Entry already exist");
            }

            this.WriteToFile(dataJson, this.GetFilePath(cardId));
        }


        public void Update(string dataJson, string cardId)
        {
            if (!this.Exists(cardId))
            {
                throw new Exception("Entry is not found.");
            }
            this.WriteToFile(dataJson, this.GetFilePath(cardId));
        }


        private void WriteToFile(string dataJson, string filePath)
        {
            var dataBytes = Encoding.UTF8.GetBytes(dataJson);

            File.WriteAllBytes(filePath, dataBytes);
        }


        public void Delete(string cardId)
        {
            if (!this.Exists(cardId))
            {
                throw new Exception("Entry is not found.");
            }

            File.Delete(this.GetFilePath(cardId));

            if (this.FileNames().Count() == 0)
            {
                Directory.Delete(this.folderPath);
            }

        }
    }
}

