using Hyper.Games.Chess.Core.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hyper.Games.Chess.Infrastructure.Helpers
{
    public class EloRatingCalculator
    {
        /// <summary>
        /// Generic elo algorithm
        /// </summary>
        public static void UpdateScores(ref int user1Rating, ref int user2Rating, GameResult result, int diff = 400, int kFactor = 10)
        {
            double est1 = 1 / Convert.ToDouble(1 + 10 ^ (user2Rating - user1Rating) / diff);
            double est2 = 1 / Convert.ToDouble(1 + 10 ^ (user1Rating - user2Rating) / diff);

            float sc1 = 0;
            float sc2 = 0;

            if (result == GameResult.Tie)
            {
                sc1 = 0.5f;
                sc2 = 0.5f;
            }
            else if (result == GameResult.WhiteWin)
            {
                sc1 = 1;
            }
            else if (result == GameResult.BlackWin)
            {
                sc2 = 1;
            }

            user1Rating = Convert.ToInt32(Math.Round(user1Rating + kFactor * (sc1 - est1)));
            user2Rating = Convert.ToInt32(Math.Round(user2Rating + kFactor * (sc2 - est2)));
        }
    }
}
