using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using Hyper.Games.Chess.Configuration.DB;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.IdGenerators;

namespace Hyper.Games.Chess.Configuration.DB
{
    public class MongoServerFactory
    {
        public static MongoServer CreateServer()
        {
            string url = ConfigurationManager.ConnectionStrings["Mongo"].ConnectionString;
            return new MongoClient(url).GetServer();
        }

        public static MongoDatabase GetDatabase(MongoServer server) {
            return server.GetDatabase("ChessDB");
        }
    }
}