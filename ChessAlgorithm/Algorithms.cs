using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessAlgorithm
{
    public class Algorithms
    {
        public static FoundGameType SeekGame(SeekedGameType gameType, List<HostedGameType> hostedGames)
        {
            foreach (var hostedGame in hostedGames)
            {
                if (hostedGame.StartTime == gameType.StartTime
                    && hostedGame.AddedTimePerMove == gameType.AddedTimePerMove
                    && hostedGame.HosterRating >= gameType.OpponentRatingFrom
                    && hostedGame.HosterRating <= gameType.OpponentRatingTo
                    && gameType.SeekerRating >= hostedGame.OpponentRatingFrom
                    && gameType.SeekerRating <= hostedGame.OpponentRatingTo)
                {
                    return new FoundGameType
                    {
                        HosterId = hostedGame.HosterId,
                        SeekerId = gameType.SeekerId,
                        StartTime = hostedGame.StartTime,
                        AddedTimePerMove = hostedGame.AddedTimePerMove
                    };
                }
            }
            return null;
        }
    }
}
