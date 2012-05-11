using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WKSADB;

namespace WKSATournament.Controllers
{
    public class HomeController : Controller
    {
        private WKSAEntities db = new WKSAEntities();

        //TODO: Homepage needs the following: open tournament (select from list), new tournament, reports, setup tournament types (events, age groups, ranks), schools list, student list (view tournament history etc)
        public ActionResult Index()
        {
            ViewBag.Message = "Modify this template to jump-start your ASP.NET MVC application.";

            ViewBag.Tournaments = db.Tournaments.Include("Venue").OrderBy(m => m.StartDate);

            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your quintessential app description page.";

            return View();
        }

        public ActionResult Admin()
        {
            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your quintessential contact page.";

            return View();
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}
