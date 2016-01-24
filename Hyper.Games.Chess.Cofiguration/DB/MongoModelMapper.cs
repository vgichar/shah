using Hyper.Games.Chess.Core;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.IdGenerators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hyper.Games.Chess.Configuration.DB
{
    public static class MongoModelMapper
    {
        public static void Activate()
        {
            BsonClassMap.RegisterClassMap<BaseModel>(cm =>
            {
                cm.MapIdProperty(c => c.Id)
                    .SetIdGenerator(new ObjectIdGenerator())
                    .SetRepresentation(BsonType.ObjectId);
            });
        }
    }
}
