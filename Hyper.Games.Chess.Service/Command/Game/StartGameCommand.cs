using Hyper.Games.Chess.Core.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hyper.Games.Chess.Service.Command.Game
{
    public class StartGameCommand : GameModel, ICommand
    {
        public StartGameCommand(string matchId, string whiteUserName, string blackUserName) : base(matchId, whiteUserName, blackUserName)
        {
        }
    }
}
