using Hyper.Games.Chess.Core.Game;
using Hyper.Games.Chess.Infrastructure.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hyper.Games.Chess.Service.Command.Game
{
    public class StartGameCommandHandler : ICommandHandler<StartGameCommand> 
    {
        private MongoRepository _mongoRepo;

        public StartGameCommandHandler(MongoRepository mongoRepo)
        {
            _mongoRepo = mongoRepo;
        }

        public void Handle(StartGameCommand command)
        {
            _mongoRepo.ActiveCollection = "ActiveGames";

            _mongoRepo.Save<GameModel>(new GameModel(command.MatchId, command.WhiteUserName, command.BlackUserName));
        }
    }
}
