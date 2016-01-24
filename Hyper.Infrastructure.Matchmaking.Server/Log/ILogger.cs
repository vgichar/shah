using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hyper.Infrastructure.Matchmaking.Server.Log
{
    public interface ILogger
    {
        bool IsActive { get; set; }

        void Log(string message);
    }
}
