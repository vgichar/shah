using Hyper.SignalR.Session.Config;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hyper.SignalR.Session.Core
{
    public class Storage : IEnumerable<KeyValuePair<string, StorageValue>>
    {
        private ConcurrentDictionary<string, StorageValue> _dictionary;

        public Storage()
        {
            _dictionary = new ConcurrentDictionary<string, StorageValue>();
        }

        public object this[string key]
        {
            get
            {
                if (_dictionary.ContainsKey(key))
                {
                    StorageValue storageValue = _dictionary[key] as StorageValue;
                    if (storageValue == null)
                    {
                        _dictionary[key] = new StorageValue(null);
                    }
                    if (storageValue.Container == null)
                    {
                        storageValue.Container = new StorageValueContainer(null);
                    }

                    if (storageValue.Provider != null)
                    {
                        if (storageValue.Provider.Mode == Provide.OnceAtStart)
                        {
                            storageValue.Provider.Invoke(storageValue.Container);
                            storageValue.Provider = null;
                            return storageValue.Container.Value;
                        }
                        else if (storageValue.Provider.Mode == Provide.Always)
                        {
                            storageValue.Provider.Invoke(storageValue.Container);
                            object value = storageValue.Container.Value;
                            storageValue.Container.Value = null;
                            return value;
                        }
                        else if (storageValue.Provider.Mode == Provide.OnceWhenNull)
                        {
                            if (storageValue.Container.Value == null)
                            {
                                storageValue.Provider.Invoke(storageValue.Container);
                                storageValue.Provider = null;
                                return storageValue.Container.Value;
                            }
                        }
                        else if (storageValue.Provider.Mode == Provide.WhenNull)
                        {
                            if (storageValue.Container.Value == null)
                            {
                                storageValue.Provider.Invoke(storageValue.Container);
                                return storageValue.Container.Value;
                            }
                        }
                    }
                    return storageValue.Container.Value;
                }
                return null;
            }
            set
            {
                if (value != null)
                {
                    if (value is StorageValue)
                    {
                        _dictionary[key] = value as StorageValue;
                    }
                    else
                    {
                        _dictionary[key] = new StorageValue(value);
                    }
                }
                else
                {
                    StorageValue val;
                    while (_dictionary.TryRemove(key, out val)) ;
                }
            }
        }

        public IEnumerator<KeyValuePair<string, StorageValue>> GetEnumerator()
        {
            foreach (KeyValuePair<string, StorageValue> value in _dictionary)
            {
                yield return value;
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
