using Hyper.SignalR.Session.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hyper.SignalR.Session.Providers
{
    public class StorageProvider : IEnumerable<KeyValuePair<string, StorageValue>>
    {
        private Storage _storage;

        public StorageProvider()
        {
            _storage = new Storage();
        }

        public Storage Current
        {
            get
            {
                return _storage;
            }
        }

        public void ClearStorage()
        {
            _storage = new Storage();
        }

        public IEnumerator<KeyValuePair<string, StorageValue>> GetEnumerator()
        {
            return _storage.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _storage.GetEnumerator();
        }
    }
}
