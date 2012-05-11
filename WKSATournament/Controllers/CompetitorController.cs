using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Lib.Web.Mvc.JQuery.JqGrid;
using WKSADB;
using WKSATournament.Extensions;

namespace WKSATournament.Controllers
{
    public class CompetitorController : BaseController
    {
        private WKSAEntities db = new WKSAEntities();

        public ActionResult Create(int tournamentId)
        {
            InitViewBag(tournamentId);

            return View("Edit");
        }

        [HttpPost]
        public ActionResult Create(Competitor competitor)
        {
            if (ModelState.IsValid)
            {
                competitor.RankId = competitor.Student.RankId;
                //competitor.AgeGroupId = db.AgeGroups.Single(m => !m.IsSparringGroup && (!m.FromAge.HasValue || m.FromAge <= competitor.Age) && (!m.ToAge.HasValue || m.ToAge >= competitor.Age)).AgeGroupId;

                foreach (CompetitorDivision competitorDivision in competitor.Divisions.Where(m => m.DivisionId != 0))
                {
                    competitor.CompetitorDivisions.Add(competitorDivision);
                }

                assignGrandChampion(competitor);

                db.Competitors.AddObject(competitor);

                // Update student record if using existing one
                if (competitor.Student.StudentId != 0)
                {
                    db.ObjectStateManager.ChangeObjectState(competitor.Student, EntityState.Modified);
                }

                //TODO: Catch errors and display on page (test by using duplicate BBID)
                db.SaveChanges();
                //TODO: Add something to the querystring to select the competitors tab
                return RedirectToAction("Details", "Tournament", new { id = competitor.TournamentId });
            }

            ViewBag.StudentId = new SelectList(db.Students, "StudentId", "WKSAId", competitor.StudentId);
            ViewBag.TournamentId = new SelectList(db.Tournaments, "TournamentId", "TournamentName", competitor.TournamentId);
            return View(competitor);
        }

        //TODO: Need to implement deletion of competitors
        public ActionResult Delete(int id = 0)
        {
            Competitor competitor = db.Competitors.Single(c => c.TournamentId == id);
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
            Competitor competitor = db.Competitors.Single(c => c.TournamentId == id);
            db.Competitors.DeleteObject(competitor);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public ActionResult Details(int id = 0)
        {
            Competitor competitor = db.Competitors.Single(c => c.CompetitorId == id);
            if (competitor == null)
            {
                return HttpNotFound();
            }
            return View(competitor);
        }

        public ActionResult Edit(int id = 0)
        {
            //TODO: Don't do auto lookup for student when editing competitors
            Competitor competitor = db.Competitors.Single(c => c.CompetitorId == id);
            if (competitor == null)
            {
                return HttpNotFound();
            }

            InitViewBag(competitor.TournamentId);

            return View(competitor);
        }

        [HttpPost]
        public ActionResult Edit(Competitor competitor)
        {
            if (ModelState.IsValid)
            {
                Competitor competitorToSave = db.Competitors.Single(m => m.CompetitorId == competitor.CompetitorId);

                competitorToSave.Age = competitor.Age;
                competitorToSave.RankId = competitor.Student.RankId;
                competitorToSave.Fee = competitor.Fee;

                competitorToSave.Student.WKSAId = competitor.Student.WKSAId;
                competitorToSave.Student.BlackBeltId = competitor.Student.BlackBeltId;
                competitorToSave.Student.SchoolId = competitor.Student.SchoolId;
                competitorToSave.Student.FirstName = competitor.Student.FirstName;
                competitorToSave.Student.LastName = competitor.Student.LastName;
                competitorToSave.Student.RankId = competitor.Student.RankId;
                competitorToSave.Student.DateOfBirth = competitor.Student.DateOfBirth;
                competitorToSave.Student.Gender = competitor.Student.Gender;

                competitorToSave.CompetitorDivisions.Clear();
                foreach (CompetitorDivision competitorDivision in competitor.Divisions.Where(m => m.DivisionId != 0))
                {
                    competitorToSave.CompetitorDivisions.Add(competitorDivision);
                }

                assignGrandChampion(competitorToSave);

                db.SaveChanges();
                return RedirectToAction("Details", "Tournament", new { id = competitor.TournamentId});
            }
            ViewBag.StudentId = new SelectList(db.Students, "StudentId", "WKSAId", competitor.StudentId);
            ViewBag.TournamentId = new SelectList(db.Tournaments, "TournamentId", "TournamentName", competitor.TournamentId);
            return View(competitor);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult GridData(JqGridRequest request, int id)
        {
            IQueryable<Competitor> competitors = db.Competitors.Where(m => m.TournamentId == id);

            if (request.Searching)
            {
                foreach (JqGridRequestSearchingFilter searchingFilter in request.SearchingFilters.Filters)
                {
                    // No idea why I have to assign this to a string var first. Doesn't work otherwise!?!
                    string searchText = searchingFilter.SearchingValue;

                    if (searchingFilter.SearchingName.Contains(WKSADBConstants.FirstName))
                    {
                        competitors = competitors.Where(m => m.Student.FirstName.Contains(searchText));
                    }
                    else if (searchingFilter.SearchingName.Contains(WKSADBConstants.LastName))
                    {
                        competitors = competitors.Where(m => m.Student.LastName.Contains(searchText));
                    }
                    else
                    {
                        int searchValue;

                        if (Int32.TryParse(searchingFilter.SearchingValue, out searchValue))
                        {
                            if (searchingFilter.SearchingName.Contains(WKSADBConstants.SchoolName))
                            {
                                competitors = competitors.Where(m => m.Student.SchoolId == searchValue);
                            }
                            else if (searchingFilter.SearchingName.Contains(WKSADBConstants.RankId))
                            {
                                competitors = competitors.Where(m => m.Student.RankId == searchValue);
                            }
                        }
                    }
                }
            }

            int totalRecords = competitors.Count();

            //Prepare JqGridData instance
            JqGridResponse response = new JqGridResponse()
            {
                //Total pages count
                TotalPagesCount = (int)Math.Ceiling((float)totalRecords / (float)request.RecordsCount),
                //Page number
                PageIndex = request.PageIndex,
                //Total records count
                TotalRecordsCount = totalRecords
            };

            //Table with rows data
            foreach (Competitor competitor in competitors.OrderBy(request.SortingName + " " + request.SortingOrder.ToString()).Skip(request.PageIndex * request.RecordsCount).Take(request.PagesCount.HasValue ? request.PagesCount.Value : 1 * request.RecordsCount))
            {
                response.Records.Add(new JqGridRecord(Convert.ToString(competitor.CompetitorId), new List<object>()
                {
                    competitor.CompetitorId,
                    competitor.Student.FirstName,
                    competitor.Student.LastName,
                    competitor.Student.Rank.Description,
                    competitor.Student.School.SchoolName
                }));
            }

            //Return data as json
            return new JqGridJsonResult() { Data = response };
        }

        private void assignGrandChampion(Competitor competitor)
        {
            competitor.GrandChampionId = null;
            if (db.GrandChampions.Any(m => m.RankId == competitor.RankId && (!m.FromAge.HasValue || m.FromAge <= competitor.Age) && (!m.ToAge.HasValue || m.ToAge >= competitor.Age) && (string.IsNullOrEmpty(m.Gender) || m.Gender.Equals(competitor.Student.Gender))))
            {
                GrandChampion grandChampion = db.GrandChampions.Single(m => m.RankId == competitor.RankId && (!m.FromAge.HasValue || m.FromAge <= competitor.Age) && (!m.ToAge.HasValue || m.ToAge >= competitor.Age) && (string.IsNullOrEmpty(m.Gender) || m.Gender.Equals(competitor.Student.Gender)));

                if (competitor.CompetitorDivisions.Count == grandChampion.DivisionCount)
                {
                    competitor.GrandChampionId = grandChampion.GrandChampionId;
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }

        private void InitViewBag(int tournamentId)
        {
            ViewBag.CountryId = new SelectList(db.Countries, "CountryId", "CountryName");
            ViewBag.RankId = new SelectList(db.Ranks, "RankId", "Description");
            ViewBag.SchoolId = new SelectList(db.Schools, "SchoolId", "FullName");
            ViewBag.DivisionTypes = db.DivisionTypes.ToList();
            ViewBag.TournamentId = tournamentId;

            Tournament tournament = db.Tournaments.Single(m => m.TournamentId == tournamentId);
            ViewBag.TournamentStartDate = tournament.StartDate;

            GetTournamentStats(tournament);
        }
    }
}