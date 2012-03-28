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
    //TODO: Add ability to edit tournament divisions. Maybe put that into the same view instead of having a create and edit one
    public class TournamentController : Controller
    {
        private WKSAEntities db = new WKSAEntities();

        //
        // GET: /Tournament/

        public ActionResult Index()
        {
            var tournaments = db.Tournaments.Include("Venue");
            return View(tournaments.ToList());
        }

        //
        // GET: /Tournament/Details/5

        public ActionResult Details(int id = 0)
        {
            Tournament tournament = db.Tournaments.Single(t => t.TournamentId == id);
            if (tournament == null)
            {
                return HttpNotFound();
            }
            return View(tournament);
        }

        //
        // GET: /Tournament/Create

        public ActionResult Create()
        {
            ViewBag.DivisionList = db.Divisions.OrderBy(m => m.RankId).ThenBy(m => m.DivisionTypeId).ThenBy(m => m.AgeGroup.FromAge).ThenBy(m => m.AgeGroup.ToAge);
            ViewBag.VenueId = new SelectList(db.Venues, "VenueId", "VenueName");
            return View();
        }

        //
        // POST: /Tournament/Create

        [HttpPost]
        public ActionResult Create(Tournament tournament)
        {
            if (ModelState.IsValid)
            {
                foreach (int divisionId in tournament.SelectedDivisionIds)
                {
                    tournament.TournamentDivisions.Add(new TournamentDivision
                    {
                        DivisionId = divisionId
                    });
                }

                db.Tournaments.AddObject(tournament);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.DivisionList = db.Divisions.OrderBy(m => m.RankId).ThenBy(m => m.DivisionTypeId).ThenBy(m => m.AgeGroup.FromAge).ThenBy(m => m.AgeGroup.ToAge);

            ViewBag.VenueId = new SelectList(db.Venues, "VenueId", "VenueName", tournament.VenueId);
            return View(tournament);
        }

        //
        // GET: /Tournament/Edit/5

        public ActionResult Edit(int id = 0)
        {
            Tournament tournament = db.Tournaments.Single(t => t.TournamentId == id);
            if (tournament == null)
            {
                return HttpNotFound();
            }
            ViewBag.VenueId = new SelectList(db.Venues, "VenueId", "VenueName", tournament.VenueId);
            return View(tournament);
        }

        //
        // POST: /Tournament/Edit/5

        [HttpPost]
        public ActionResult Edit(Tournament tournament)
        {
            if (ModelState.IsValid)
            {
                db.Tournaments.Attach(tournament);
                db.ObjectStateManager.ChangeObjectState(tournament, EntityState.Modified);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.VenueId = new SelectList(db.Venues, "VenueId", "VenueName", tournament.VenueId);
            return View(tournament);
        }

        //
        // GET: /Tournament/Delete/5

        public ActionResult Delete(int id = 0)
        {
            Tournament tournament = db.Tournaments.Single(t => t.TournamentId == id);
            if (tournament == null)
            {
                return HttpNotFound();
            }
            return View(tournament);
        }

        //
        // POST: /Tournament/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            Tournament tournament = db.Tournaments.Single(t => t.TournamentId == id);
            db.Tournaments.DeleteObject(tournament);
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