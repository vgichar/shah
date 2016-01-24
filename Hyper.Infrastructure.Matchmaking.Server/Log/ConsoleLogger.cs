using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hyper.Infrastructure.Matchmaking.Server.Log
{
    public class ConsoleLogger : ILogger
    {
        private static ILogger _instance;

        private ConsoleLogger()
        {
            IsActive = false;
        }

        public bool IsActive { get; set; }

        public static ILogger Current
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ConsoleLogger();
                }
                return _instance;
            }
        }

        public void Log(string message)
        {
            if (IsActive)
            {
                string printableMessage = string.Format("[{0}]: {1}", DateTime.Now, message);
                Console.WriteLine(printableMessage);
            }
        }
    }
}
