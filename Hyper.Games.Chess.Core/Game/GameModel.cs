using Hyper.Games.Chess.Core.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Hyper.Games.Chess.Core.Game
{
    public class GameModel : BaseModel
    {
        public GameModel(string matchId, string whiteUserName, string blackUserName)
        {
            MatchId = matchId;
            WhiteUserName = whiteUserName;
            BlackUserName = blackUserName;
            Result = GameResult.NoResult;
            Moves = new List<GameMoveModel>();
            GameStartTime = DateTime.Now;
        }

        public string MatchId { get; set; }

        public string WhiteUserName { get; set; }

        public string BlackUserName { get; set; }

        public List<GameMoveModel> Moves { get; set; }

        public GameResult Result { get; set; }

        public DateTime GameStartTime { get; set; }
    }

    public enum GameResult
    {
        NoResult = 0,
        WhiteWin = 1,
        BlackWin = 2,
        Tie = 3,
    }
}