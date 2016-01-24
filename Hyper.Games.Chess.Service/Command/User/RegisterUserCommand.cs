using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hyper.Games.Chess.Service.Command.User
{
    public class RegisterUserCommand : ICommand
    {
        public RegisterUserCommand(string userName)
        {
            UserName = userName;
        }

        public string UserName { get; set; }
    }
}
