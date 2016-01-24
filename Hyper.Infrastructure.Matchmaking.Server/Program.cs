using Hyper.Infrastructure.Matchmaking.Server.Log;
using Microsoft.Owin.Hosting;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hyper.Infrastructure.Matchmaking.Server
{
    public class Program
    {
        static void Main(string[] args)
        {
            bool doLog = false;
            bool.TryParse(ConfigurationManager.AppSettings["Hyper.Log"], out doLog);

            ConsoleLogger.Current.IsActive = doLog;

            string url = ConfigurationManager.AppSettings["Hyper.MatchmakingServerUrl"];
            using (WebApp.Start(url))
            {
                Console.WriteLine("Server running on {0}", url);
                while (true)
                {
                    Console.ReadLine();
                    ConsoleLogger.Current.IsActive = !ConsoleLogger.Current.IsActive;
                }
            }
        }
    }
}
