using Hyper.SignalR.Session;
using Hyper.SignalR.Session.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Hyper.Games.Chess.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        // [MigrateSession(true)]
        public ActionResult Index()
        {
            return View();
        }
    }
}