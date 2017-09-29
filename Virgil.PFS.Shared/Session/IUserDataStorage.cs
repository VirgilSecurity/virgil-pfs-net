using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Virgil.PFS.Session
{
    public interface IUserDataStorage
    {
        void Save(string sessionStateJson, string cardId);
        string Load(string cardId);
        void Delete(string cardId);
        bool Exists(string cardId);
        string[] LoadAllNames();
        void Update(string dataJson, string cardId);
    }
}
