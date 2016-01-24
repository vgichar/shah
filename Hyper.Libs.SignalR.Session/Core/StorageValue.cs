using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hyper.SignalR.Session.Core
{
    public class StorageValue
    {
        public StorageValue(object value, StorageValueProvider provider = null)
        {
            HandleValue(value);
            HandleProvider(provider);
        }

        public StorageValue(object value, Action<StorageValueContainer> provider)
        {
            HandleValue(value);
            HandleProvider(provider);
        }

        private void HandleValue(object value)
        {
            if (value == null)
            {
                Container = new StorageValueContainer(null);
            }
            else if (value is StorageValueContainer)
            {
                Container = value as StorageValueContainer;
            }
            else if (value is StorageValueProvider)
            {
                Container = new StorageValueContainer(null);
                Provider = value as StorageValueProvider;
            }
            else
            {
                Container = new StorageValueContainer(value);
            }
        }

        private void HandleProvider(object provider)
        {
            if (provider is Action<StorageValueContainer>)
            {
                Provider = new StorageValueProvider(provider as Action<StorageValueContainer>);
            }
            else if (provider is StorageValueProvider)
            {
                Provider = provider as StorageValueProvider;
            }
            else
            {
                Provider = null;
            }
        }

        public StorageValueContainer Container { get; set; }

        public StorageValueProvider Provider { get; set; }
    }
}
