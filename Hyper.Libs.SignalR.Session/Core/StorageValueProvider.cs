using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hyper.SignalR.Session.Core
{
    public class StorageValueProvider
    {
        private Action<StorageValueContainer> _provider;
        public Provide Mode { get; set; }

        public StorageValueProvider(Action<StorageValueContainer> provider, Provide mode = Provide.WhenNull)
        {
            _provider = provider;
            Mode = mode;
        }

        public void Invoke(StorageValueContainer container)
        {
            _provider.Invoke(container);
        }
    }

    public enum Provide
    {
        OnceAtStart, OnceWhenNull, WhenNull, Always
    }
}
