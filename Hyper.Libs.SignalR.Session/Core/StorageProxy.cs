using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hyper.SignalR.Session.Core
{
    internal class StorageProxy : DynamicObject
    {
        private Storage _storage;

        public StorageProxy(Storage session)
        {
            _storage = session;
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            _storage[binder.Name] = value;
            return true;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            result = _storage[binder.Name];
            return true;
        }
    }
}
