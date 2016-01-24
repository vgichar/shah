using Hyper.Games.Chess.Core.User;
using Hyper.Games.Chess.Infrastructure.DB;
using Hyper.Games.Chess.Service.DTO.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hyper.Games.Chess.Service.Query.User
{
    public class GetUserByUserNameQueryHandler : IQueryHandler<UserDto, GetUserByUserNameQuery>
    {
        private MongoRepository _repo;

        public GetUserByUserNameQueryHandler(MongoRepository repo)
        {
            _repo = repo;
        }

        /// <summary>
        /// Warning! Ids is not populated
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public UserDto Handle(GetUserByUserNameQuery query)
        {
            _repo.ActiveCollection = "Users";
            var user = _repo.Get<UserModel>(x=>x.UserName == query.UserName);
            return new UserDto
            {
                Rating = user.Rating,
                UserName = user.UserName
            };
        }
    }
}
