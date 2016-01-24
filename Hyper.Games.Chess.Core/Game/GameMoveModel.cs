using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hyper.Games.Chess.Core.Game
{
    public class GameMoveModel
    {
        public GameMoveModel(string from, string to, string promotionPiece = "")
        {
            From = from;
            To = to;
            PromotionPiece = promotionPiece;
            Time = DateTime.Now;
        }

        public string From { get; set; }

        public string To { get; set; }

        public string PromotionPiece { get; set; }

        public DateTime Time { get; set; }
    }
}
