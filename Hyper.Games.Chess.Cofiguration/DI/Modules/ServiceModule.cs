using Autofac;
using Hyper.Games.Chess.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hyper.Games.Chess.Configuration.DI.Modules
{
    public class ServiceModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterAssemblyTypes(typeof(IQueryHandler<,>).Assembly).Where(x => x.Name.EndsWith("Handler")).AsImplementedInterfaces().AsSelf().SingleInstance();
            base.Load(builder);
        }
    }
}
