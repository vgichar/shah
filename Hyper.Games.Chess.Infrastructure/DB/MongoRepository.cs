using Hyper.Games.Chess.Core;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Hyper.Games.Chess.Infrastructure.DB
{
    public class MongoRepository
    {
        private MongoDatabase _db;
        public string ActiveCollection { get; set; }

        public MongoRepository(MongoDatabase db)
        {
            _db = db;
        }

        public void Save<T>(T obj) where T : BaseModel
        {
            _db.GetCollection(ActiveCollection).Save<T>(obj);
        }

        public T Get<T>(object id) where T : BaseModel
        {
            return _db.GetCollection(ActiveCollection).FindOneByIdAs<T>(id.ToString());
        }

        public T Get<T>(Expression<Func<T, bool>> expr) where T : BaseModel
        {
            IMongoQuery query = Query<T>.Where(expr);
            return _db.GetCollection(ActiveCollection).FindOneAs<T>(query);
        }

        public List<T> All<T>(Expression<Func<T, bool>> expr = null) where T : BaseModel
        {
            if (expr != null)
            {
                IMongoQuery query = Query<T>.Where(expr);
                return _db.GetCollection(ActiveCollection).FindAs<T>(query).ToList<T>();
            }
            return _db.GetCollection(ActiveCollection).FindAllAs<T>().ToList<T>();
        }

        public void Remove<T>(object Id) where T : BaseModel
        {
            IMongoQuery query = Query<T>.EQ<string>(x => x.Id.ToString(), Id.ToString());
            _db.GetCollection(ActiveCollection).Remove(query);
        }

        public void Remove<T>(BaseModel obj) where T : BaseModel
        {
            IMongoQuery query = Query<T>.EQ<string>(x => x.Id.ToString(), obj.Id.ToString());
            _db.GetCollection(ActiveCollection).Remove(query);
        }
    }
}
