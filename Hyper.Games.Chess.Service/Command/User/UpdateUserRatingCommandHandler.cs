using Hyper.Games.Chess.Core.User;
using Hyper.Games.Chess.Infrastructure.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hyper.Games.Chess.Service.Command.User
{
    public class UpdateUserRatingCommandHandler : ICommandHandler<UpdateUserRatingCommand>
    {
        private MongoRepository _mongoRepo;

        public UpdateUserRatingCommandHandler(MongoRepository mongoRepo)
        {
            _mongoRepo = mongoRepo;
        }

        public void Handle(UpdateUserRatingCommand command)
        {
            _mongoRepo.ActiveCollection = "Users";
            UserModel user = _mongoRepo.Get<UserModel>(x => x.UserName == command.UserName);
            user.Rating = command.Rating;
            _mongoRepo.Save<UserModel>(user);
        }
    }
}
