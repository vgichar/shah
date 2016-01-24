using Hyper.Games.Chess.Core.Game;
using Hyper.Games.Chess.Infrastructure.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hyper.Games.Chess.Service.Command.Game
{
    public class MakeMoveCommandHandler : ICommandHandler<MakeMoveCommand>
    {
        private MongoRepository _mongoRepo;

        public MakeMoveCommandHandler(MongoRepository mongoRepo)
        {
            _mongoRepo = mongoRepo;
        }

        public void Handle(MakeMoveCommand command)
        {
            _mongoRepo.ActiveCollection = "ActiveGames";
            var game = _mongoRepo.Get<GameModel>(x => x.MatchId == command.MatchId);
            
            game.Moves.Add(new GameMoveModel(command.From, command.To, command.PromotionPiece));

            _mongoRepo.Save<GameModel>(game);
        }
    }
}
