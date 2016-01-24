using Hyper.Games.Chess.Core.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hyper.Games.Chess.Service.Command.Game
{
    public class UpdateGameResultCommand : ICommand
    {
        public UpdateGameResultCommand(string matchId, GameResult result)
        {
            MatchId = matchId;
            Result = result;
        }

        public string MatchId { get; set; }

        public GameResult Result { get; set; }
    }
}
