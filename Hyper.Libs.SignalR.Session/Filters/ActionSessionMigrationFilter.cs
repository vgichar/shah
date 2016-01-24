using Hyper.SignalR.Session.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Hyper.SignalR.Session.Filters
{
    internal class ActionSessionMigrationFilter : ActionFilterAttribute
    {
        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            MigrateSession migrateSessionAttribute = filterContext.ActionDescriptor.GetCustomAttributes(typeof(MigrateSession), false).OfType<MigrateSession>().SingleOrDefault();
            bool hasMigrateSessionAttribute = migrateSessionAttribute != null;

            if (hasMigrateSessionAttribute)
            {
                MigrateSession.HttpToSignalR(migrateSessionAttribute);
            }
            base.OnActionExecuted(filterContext);
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            MigrateSession.SignalRToHttp();

            base.OnActionExecuting(filterContext);
        }
    }
}
