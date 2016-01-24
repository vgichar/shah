using Hyper.Games.Chess.Core.User;
using Hyper.Games.Chess.Infrastructure.DB;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hyper.Games.Chess.Service.Command.User
{
    public class RegisterUserCommandHandler : ICommandHandler<RegisterUserCommand>
    {
        private MongoRepository _repo;

        public RegisterUserCommandHandler(MongoRepository repo)
        {
            _repo = repo;
        }

        public void Handle(RegisterUserCommand command)
        {
            _repo.ActiveCollection = "Users";
            _repo.Save<UserModel>(new UserModel
            {
                UserName = command.UserName,
                Rating = 1500
            });
        }
    }
}
