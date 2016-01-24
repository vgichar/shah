using ChessAlgorithm;
using Hyper.Infrastructure.Matchmaking.Server.Log;
using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hyper.Infrastructure.Matchmaking.Server.Queue
{
    public class MatchQueueHub : Hub
    {
        public override Task OnConnected()
        {
            ConsoleLogger.Current.Log("Connected " + Context.ConnectionId);

            MatchQueue.MatchFoundEvent += (object sender, EventArgs e) =>
            {
                ReportFoundMatch();
            };

            return base.OnConnected();
        }

        public void FindMatch(SeekedGameType seekedGameType)
        {
            MatchQueue.Enqueue(seekedGameType);
        }

        public void StopFindMatch(string connectionId)
        {
            MatchQueue.Dequeue(connectionId);
        }

        public void ReportFoundMatch()
        {
            FoundGameType match;
            match = MatchQueue.PopMatch();
            Clients.Caller.reportMatch(match);
        }
    }
}
