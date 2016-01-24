using Hyper.Games.Chess.Core.Game;
using Hyper.Games.Chess.Service.DTO.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hyper.Games.Chess.Service.Command.Game
{
    public class MakeMoveCommand : GameMoveDto, ICommand
    {
        public MakeMoveCommand(string matchId, string from, string to, string promotionPiece = "", GameResult gameResult = GameResult.NoResult)
            : base(from, to, promotionPiece)
        {
            MatchId = matchId;
            GameResult = gameResult;
        }

        public string MatchId { get; set; }

        public GameResult GameResult { get; set; }
    }
}
