using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hyper.SignalR.Session.Core
{
    public class StorageValueContainer
    {
        public object Value { get; set; }

        public StorageValueContainer(object value)
        {
            Value = value;
        }
    }
}
