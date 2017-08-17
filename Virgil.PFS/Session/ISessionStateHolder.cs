using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Virgil.PFS
{
    public interface ISessionStateHolder
    {
        void Save(string sessionStateJson, string cardId);
        string Load(string cardId);
        void Delete(string cardId);
        void DeleteAll();
        bool Exists(string cardId);
        string[] LoadAll();
        string[] LoadAllNames();
    }
}
