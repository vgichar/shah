using Hyper.Games.Chess.Core.Game;
using Hyper.Games.Chess.Infrastructure.DB;
using Hyper.Games.Chess.Service.DTO.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hyper.Games.Chess.Service.Query.Game
{
    public class GetGameMovesQueryHandler : IQueryHandler<List<GameMoveDto>, GetGameMovesQuery>
    {
        private MongoRepository _mongoRepo;

        public GetGameMovesQueryHandler(MongoRepository mongoRepo)
        {
            _mongoRepo = mongoRepo;
        }

        public List<GameMoveDto> Handle(GetGameMovesQuery query)
        {
            _mongoRepo.ActiveCollection = "ActiveGames";
            var game = _mongoRepo.Get<GameModel>(x => x.MatchId == query.MatchId);

            return game.Moves.Select(x => new GameMoveDto(x.From, x.To, x.PromotionPiece) { Time = x.Time }).ToList();
        }
    }
}
