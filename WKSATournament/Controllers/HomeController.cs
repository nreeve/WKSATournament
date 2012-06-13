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

        public ActionResult Index()
        {
            ViewBag.Tournaments = db.Tournaments.Include("Venue").OrderBy(m => m.StartDate);

            return View();
        }

        public ActionResult About()
        {
            return View();
        }

        public ActionResult Admin()
        {
            return View();
        }

        public ActionResult Contact()
        {
            return View();
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}
