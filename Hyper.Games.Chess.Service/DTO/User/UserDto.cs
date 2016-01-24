using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hyper.Games.Chess.Service.DTO.User
{
    public class UserDto
    {
        public string SessionId { get; set; }

        public string ConnectionId { get; set; }

        public string UserName { get; set; }

        public int Rating { get; set; }
    }
}
