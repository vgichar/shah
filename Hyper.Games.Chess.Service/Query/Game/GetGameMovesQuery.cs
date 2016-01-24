using Hyper.Games.Chess.Service.DTO.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hyper.Games.Chess.Service.Query.Game
{
    public class GetGameMovesQuery : IQuery<List<GameMoveDto>>
    {
        public GetGameMovesQuery(string matchId)
        {
            MatchId = matchId;
        }

        public string MatchId { get; set; }
    }
}
