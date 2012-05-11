using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using WKSADB;
using WKSATournament.Extensions;
using WKSATournament.Models;

namespace WKSATournament.Controllers
{
    //TODO: Look into SignalR for handheld app
    //TODO: All pages to do with a tournament need a header like in the main home page that shows stats for the tournament: total competitors, divisions completed/medals given (e.g. 18/250)
    public class TournamentController : BaseController
    {
        private WKSAEntities db = new WKSAEntities();

        public ActionResult Index()
        {
            var tournaments = db.Tournaments.Include("Venue");
            return View(tournaments.ToList());
        }

        public ActionResult Create()
        {
            initViewBag(new Tournament());
            return View("Edit");
        }

        //TODO: Not sure if this works correctly with the divisions/grand champions. Test
        [HttpPost]
        public ActionResult Create(Tournament tournament)
        {
            if (ModelState.IsValid)
            {
                populateDivisions(tournament);

                db.Tournaments.AddObject(tournament);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            else
            {
                initViewBag(tournament);
            }

            return View(tournament);
        }

        //TODO: Delete Tournament. Put all deletes into popup dialog
        public ActionResult Delete(int id = 0)
        {
            Tournament tournament = db.Tournaments.Single(t => t.TournamentId == id);
            if (tournament == null)
            {
                return HttpNotFound();
            }
            return View(tournament);
        }

        //TODO: Make sure delete removes everything (divisions, competitors etc.). Needs a mega 'are you sure'
        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            Tournament tournament = db.Tournaments.Single(t => t.TournamentId == id);
            tournament.TournamentDivisions.Clear();
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        //TODO: Reports needed (grand champions, statistics etc)
        //TODO: Header needs a way of getting back to details page
        public ActionResult Details(int id = 0)
        {
            Tournament tournament = db.Tournaments.Single(t => t.TournamentId == id);
            if (tournament == null)
            {
                return HttpNotFound();
            }

            GetTournamentStats(tournament);

            //TODO: Maybe do a cached appResources type thing for these items so not hitting the DB each time
            IEnumerable<Rank> rankList = db.Ranks;
            IEnumerable<DivisionType> divisionTypeList = db.DivisionTypes;
            IEnumerable<AgeGroup> ageGroupList = db.AgeGroups;
            ViewBag.RankId = new SelectList(rankList, "RankId", "Description");
            ViewBag.DivisionTypeId = new SelectList(divisionTypeList, "DivisionTypeId", "Description");
            ViewBag.AgeGroupId = new SelectList(ageGroupList, "AgeGroupId", "Description");

            ViewBag.RankFilter = rankList.CreateJQGridFilter(WKSADBConstants.RankId, WKSADBConstants.Description);
            ViewBag.DivisionTypeFilter = divisionTypeList.CreateJQGridFilter(WKSADBConstants.DivisionTypeId, WKSADBConstants.Description);
            ViewBag.AgeGroupFilter = ageGroupList.CreateJQGridFilter(WKSADBConstants.AgeGroupId, WKSADBConstants.Description);
            ViewBag.SchoolFilter = db.Schools.OrderBy(m => m.SchoolName).CreateJQGridFilter(WKSADBConstants.SchoolId, WKSADBConstants.SchoolName);
            
            return View(tournament);
        }

        public ActionResult Edit(int id = 0)
        {
            Tournament tournament = db.Tournaments.Single(t => t.TournamentId == id);
            if (tournament == null)
            {
                return HttpNotFound();
            }

            initViewBag(tournament);

            return View(tournament);
        }

        [HttpPost]
        public ActionResult Edit(Tournament tournament)
        {
            if (ModelState.IsValid)
            {
                db.Tournaments.Attach(tournament);

                populateDivisions(tournament);
                populateGrandChampions(tournament, db);

                db.ObjectStateManager.ChangeObjectState(tournament, EntityState.Modified);
                db.SaveChanges();
                return RedirectToAction("Details", new { id = tournament.TournamentId });
            }
            else
            {
                initViewBag(tournament);
            }

            return View(tournament);
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }

        private void initViewBag(Tournament tournament)
        {
            GetTournamentStats(tournament);
            ViewBag.DivisionList = db.Divisions.OrderBy(m => m.RankId).ThenBy(m => m.DivisionTypeId).ThenBy(m => m.AgeGroup.FromAge).ThenBy(m => m.AgeGroup.ToAge);
            ViewBag.VenueId = new SelectList(db.Venues, "VenueId", "VenueName", tournament.VenueId);
            ViewBag.GrandChampionList = db.GrandChampions.OrderBy(m => m.RankId);
        }

        private static void populateDivisions(Tournament tournament)
        {
            if (tournament.SelectedDivisionIds != null)
            {
                foreach (int divisionId in tournament.SelectedDivisionIds)
                {
                    if (!tournament.TournamentDivisions.Any(m => m.DivisionId == divisionId))
                    {
                        tournament.TournamentDivisions.Add(new TournamentDivision
                        {
                            TournamentId = tournament.TournamentId,
                            DivisionId = divisionId
                        });
                    }
                }
            }
        }

        private static void populateGrandChampions(Tournament tournament, WKSAEntities db)
        {
            tournament.GrandChampions.Clear();
            if (tournament.SelectedGrandChampionIds != null)
            {
                foreach (int grandChampionId in tournament.SelectedGrandChampionIds)
                {
                    if (!tournament.GrandChampions.Any(m => m.GrandChampionId == grandChampionId))
                    {
                        tournament.GrandChampions.Add(db.GrandChampions.Single(m => m.GrandChampionId == grandChampionId));
                    }
                }
            }
        }
    }
}