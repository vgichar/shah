using Hyper.Games.Chess.Service.DTO.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hyper.Games.Chess.Service.Query.User
{
    public class GetUserByUserNameQuery : IQuery<UserDto>
    {
        public GetUserByUserNameQuery(string userName)
        {
            UserName = userName;
        }

        public string UserName { get; set; }
    }
}
