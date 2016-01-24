using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hyper.Games.Chess.Core.User
{
    public class UserModel : BaseModel
    {
        public string UserName { get; set; }

        public int Rating { get; set; }
    }
}
