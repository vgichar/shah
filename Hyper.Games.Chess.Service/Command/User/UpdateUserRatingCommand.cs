using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hyper.Games.Chess.Service.Command.User
{
    public class UpdateUserRatingCommand : ICommand
    {
        public UpdateUserRatingCommand(string userName, int rating)
        {
            UserName = userName;
            Rating = rating;
        }

        public string UserName { get; set; }

        public int Rating { get; set; }
    }
}
