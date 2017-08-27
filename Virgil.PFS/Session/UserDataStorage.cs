using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Virgil.PFS.Session
{
    class UserDataStorage : IUserDataStorage
    {
        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            throw new NotImplementedException();

        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(KeyValuePair<string, string> item)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(KeyValuePair<string, string> item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(KeyValuePair<string, string>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public bool Remove(KeyValuePair<string, string> item)
        {
            throw new NotImplementedException();
        }

        public int Count { get; }
        public bool IsReadOnly { get; }
        public void Add(string key, string value)
        {
            throw new NotImplementedException();
        }

        public bool ContainsKey(string key)
        {
            throw new NotImplementedException();
        }

        public bool Remove(string key)
        {
            throw new NotImplementedException();
        }

        public bool TryGetValue(string key, out string value)
        {
            throw new NotImplementedException();
        }

        public string this[string key]
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public ICollection<string> Keys { get; }
        public ICollection<string> Values { get; }
    }
}
