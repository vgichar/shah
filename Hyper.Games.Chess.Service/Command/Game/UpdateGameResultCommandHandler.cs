using Hyper.Games.Chess.Core.Game;
using Hyper.Games.Chess.Infrastructure.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hyper.Games.Chess.Service.Command.Game
{
    public class UpdateGameResultCommandHandler : ICommandHandler<UpdateGameResultCommand>
    {
        private MongoRepository _mongoRepo;

        public UpdateGameResultCommandHandler(MongoRepository mongoRepo)
        {
            _mongoRepo = mongoRepo;
        }

        public void Handle(UpdateGameResultCommand command)
        {
            _mongoRepo.ActiveCollection = "ActiveGames";
            var game = _mongoRepo.Get<GameModel>(x => x.MatchId == command.MatchId);

            if (game.Result == GameResult.NoResult && command.Result != GameResult.NoResult)
            {
                game.Result = command.Result;
                _mongoRepo.Save<GameModel>(game);
            }
        }
    }
}
