using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Virgil.PFS
{
    internal interface ISessionStateHolder
    {
        void Save(string sessionStateJson, string cardId);
        string Load(string cardId);
        void Delete(string cardId);
        bool Exists(string cardId);
        string[] LoadAll();
        string[] LoadAllNames();
    }
}
