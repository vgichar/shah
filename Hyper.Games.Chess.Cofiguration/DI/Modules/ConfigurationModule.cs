using Autofac;
using Hyper.Games.Chess.Configuration.DB;
using Hyper.Games.Chess.Infrastructure.DB;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hyper.Games.Chess.Configuration.DI.Modules
{
    public class ConfigurationModule : Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.Register<MongoServer>((container) =>
            {
                return MongoServerFactory.CreateServer();
            }).SingleInstance();

            builder.Register<MongoDatabase>((container) =>
            {
                return MongoServerFactory.GetDatabase(container.Resolve<MongoServer>());
            }).SingleInstance();

            builder.Register<MongoRepository>((container) =>
            {
                var db = container.Resolve<MongoDatabase>();
                return new MongoRepository(db);
            }).SingleInstance();

            base.Load(builder);
        }
    }
}
