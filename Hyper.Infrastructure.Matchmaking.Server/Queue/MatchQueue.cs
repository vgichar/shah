using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using Hyper.Infrastructure.Matchmaking.Server.Log;
using System.Threading;
using ChessAlgorithm;

namespace Hyper.Infrastructure.Matchmaking.Server.Queue
{
    public static class MatchQueue
    {
        private static List<string> _pendingPlayers = new List<string>();
        private static SortedDictionary<DateTime, HostedGameType> _pendingMatches = new SortedDictionary<DateTime, HostedGameType>();
        private static List<FoundGameType> _madeMatches = new List<FoundGameType>();

        public static event EventHandler MatchFoundEvent;

        public static void Enqueue(SeekedGameType seekedGameType)
        {
            lock (_pendingMatches)
            {
                if (!_pendingPlayers.Where(x => x == seekedGameType.SeekerId).Any())
                {
                    _pendingPlayers.Add(seekedGameType.SeekerId);
                    ConsoleLogger.Current.Log("Find match for " + seekedGameType.SeekerId);
                    FindMatch(seekedGameType);
                }
            }
        }

        public static void Dequeue(string connectionId)
        {
            lock (_pendingMatches)
            {
                var match = _pendingMatches.Where(x => x.Value.HosterId == connectionId).FirstOrDefault();
                var matchDate = _pendingMatches.Where(x => x.Value.HosterId == connectionId).FirstOrDefault().Key;
                var matchItem = _pendingMatches.Where(x => x.Value.HosterId == connectionId).FirstOrDefault().Value;

                _pendingPlayers.Remove(connectionId);

                if (matchItem != null)
                {
                    _pendingMatches.Remove(matchDate);
                }

                ConsoleLogger.Current.Log("Stop finding match for " + connectionId);
            }
        }

        public static FoundGameType PopMatch()
        {
            lock (_pendingMatches)
            {
                if (_madeMatches.Any())
                {
                    var match = _madeMatches.First();
                    _madeMatches.RemoveAt(0);
                    return match;
                }
                return null;
            }
        }

        private static void FindMatch(SeekedGameType seekedGameType)
        {
            lock (_pendingMatches)
            {
                FoundGameType foundMatch = Algorithms.SeekGame(seekedGameType, _pendingMatches.Values.ToList());

                if (foundMatch != null)
                {
                    var foundMatch_PendingMatchPairInstance = _pendingMatches.First(x => x.Value.HosterId == foundMatch.HosterId);

                    _madeMatches.Add(foundMatch);
                    _pendingMatches.Remove(foundMatch_PendingMatchPairInstance.Key);
                    _pendingPlayers.Remove(foundMatch.HosterId);
                    _pendingPlayers.Remove(foundMatch.SeekerId);

                    ConsoleLogger.Current.Log(foundMatch.ToString());
                    if (MatchFoundEvent != null)
                    {
                        MatchFoundEvent.Invoke(foundMatch, new EventArgs());
                    }
                }
                else
                {
                    _pendingMatches[DateTime.Now] = new HostedGameType
                    {
                        HosterId = seekedGameType.SeekerId,
                        HosterRating = seekedGameType.SeekerRating,
                        StartTime = seekedGameType.StartTime,
                        AddedTimePerMove = seekedGameType.AddedTimePerMove,
                        OpponentRatingFrom = seekedGameType.OpponentRatingFrom,
                        OpponentRatingTo = seekedGameType.OpponentRatingTo
                    };
                }
            }
        }
    }
}
