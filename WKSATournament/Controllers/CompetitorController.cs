using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WKSADB;

namespace WKSATournament.Controllers
{
    public class CompetitorController : Controller
    {
        private WKSAEntities db = new WKSAEntities();

        //
        // GET: /Competitor/

        public ActionResult Index()
        {
            var competitors = db.Competitors.Include("Student").Include("Tournament");
            return View(competitors.ToList());
        }

        //
        // GET: /Competitor/Details/5

        public ActionResult Details(int id = 0)
        {
            Competitor competitor = db.Competitors.Single(c => c.CompetitorId == id);
            if (competitor == null)
            {
                return HttpNotFound();
            }
            return View(competitor);
        }

        //
        // GET: /Competitor/Create

        public ActionResult Create(int tournamentId)
        {
            ViewBag.CountryId = new SelectList(db.Countries, "CountryId", "CountryName");
            ViewBag.RankId = new SelectList(db.Ranks, "RankId", "Description");
            ViewBag.SchoolId = new SelectList(db.Schools, "SchoolId", "FullName");
            ViewBag.DivisionTypes = db.DivisionTypes.ToList();
            ViewBag.TournamentId = tournamentId;
            ViewBag.TournamentStartDate = db.Tournaments.Single(m => m.TournamentId == tournamentId).StartDate;
            
            return View();
        }

        //
        // POST: /Competitor/Create

        [HttpPost]
        public ActionResult Create(Competitor competitor)
        {
            if (ModelState.IsValid)
            {
                db.Competitors.AddObject(competitor);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.StudentId = new SelectList(db.Students, "StudentId", "WKSAId", competitor.StudentId);
            ViewBag.TournamentId = new SelectList(db.Tournaments, "TournamentId", "TournamentName", competitor.TournamentId);
            return View(competitor);
        }

        //
        // GET: /Competitor/Edit/5

        public ActionResult Edit(int id = 0)
        {
            Competitor competitor = db.Competitors.Single(c => c.CompetitorId == id);
            if (competitor == null)
            {
                return HttpNotFound();
            }
            ViewBag.StudentId = new SelectList(db.Students, "StudentId", "WKSAId", competitor.StudentId);
            ViewBag.TournamentId = new SelectList(db.Tournaments, "TournamentId", "TournamentName", competitor.TournamentId);
            return View(competitor);
        }

        //
        // POST: /Competitor/Edit/5

        [HttpPost]
        public ActionResult Edit(Competitor competitor)
        {
            if (ModelState.IsValid)
            {
                db.Competitors.Attach(competitor);
                db.ObjectStateManager.ChangeObjectState(competitor, EntityState.Modified);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.StudentId = new SelectList(db.Students, "StudentId", "WKSAId", competitor.StudentId);
            ViewBag.TournamentId = new SelectList(db.Tournaments, "TournamentId", "TournamentName", competitor.TournamentId);
            return View(competitor);
        }

        //
        // GET: /Competitor/Delete/5

        public ActionResult Delete(int id = 0)
        {
            Competitor competitor = db.Competitors.Single(c => c.CompetitorId == id);
            if (competitor == null)
            {
                return HttpNotFound();
            }
            return View(competitor);
        }

        //
        // POST: /Competitor/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            Competitor competitor = db.Competitors.Single(c => c.CompetitorId == id);
            db.Competitors.DeleteObject(competitor);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}