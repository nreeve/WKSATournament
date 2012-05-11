using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Objects.SqlClient;
using Lib.Web.Mvc.JQuery.JqGrid;
using System.Text;
using WKSADB;
using WKSATournament.Extensions;

namespace WKSATournament.Controllers
{
    public class TournamentDivisionController : BaseController
    {
        private WKSAEntities db = new WKSAEntities();

        public ActionResult Index(int tournamentId)
        {
            if(!db.Tournaments.Any(m => m.TournamentId == tournamentId))
            {
                return HttpNotFound();
            }
            
            GetTournamentStats(db.Tournaments.Single(t => t.TournamentId == tournamentId));

            var tournamentdivisions = db.TournamentDivisions.Where(m => m.TournamentId == tournamentId).Include("Division").Include("Tournament");
            return View(tournamentdivisions.ToList());
        }

        //TODO: This needed?
        public ActionResult Delete(int id = 0)
        {
            TournamentDivision tournamentdivision = db.TournamentDivisions.Single(t => t.TournamentId == id);
            if (tournamentdivision == null)
            {
                return HttpNotFound();
            }
            return View(tournamentdivision);
        }

        //TODO: This needed?
        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            TournamentDivision tournamentdivision = db.TournamentDivisions.Single(t => t.TournamentId == id);
            db.TournamentDivisions.DeleteObject(tournamentdivision);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        //TODO: This needed?
        public ActionResult Details(int id = 0)
        {
            TournamentDivision tournamentdivision = db.TournamentDivisions.Single(t => t.TournamentId == id);
            if (tournamentdivision == null)
            {
                return HttpNotFound();
            }
            return View(tournamentdivision);
        }

        //TODO: Allow user to enter number for results or pick medal somehow?
        //TODO: Need to catch invalid division id entered in tournament home page
        public ActionResult Edit(int id = 0, int divisionId = 0)
        {
            TournamentDivision tournamentdivision = db.TournamentDivisions.Single(t => t.TournamentId == id && t.DivisionId == divisionId);
            if (tournamentdivision == null)
            {
                return HttpNotFound();
            }
            
            InitViewBag(tournamentdivision);
            return View(tournamentdivision);
        }

        [HttpPost]
        public ActionResult Edit(TournamentDivision tournamentdivision, IEnumerable<CompetitorDivision> competitors)
        {
            if (ModelState.IsValid)
            {
                foreach(CompetitorDivision competitorDivision in competitors)
                {
                    db.CompetitorDivisions.Attach(competitorDivision);
                    db.ObjectStateManager.ChangeObjectState(competitorDivision, EntityState.Modified);
                }

                db.TournamentDivisions.Attach(tournamentdivision);
                db.ObjectStateManager.ChangeObjectState(tournamentdivision, EntityState.Modified);
                db.SaveChanges();

                return RedirectToAction("Details", "Tournament", new { id = tournamentdivision.TournamentId});
            }
            InitViewBag(tournamentdivision);
            return View(tournamentdivision);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult GridData(JqGridRequest request, int id)
        {
            IQueryable<TournamentDivision> tournamentDivisions = db.TournamentDivisions.Where(m => m.TournamentId == id);
            
            if (request.Searching)
            {
                foreach (JqGridRequestSearchingFilter searchingFilter in request.SearchingFilters.Filters)
                {
                    int searchValue;

                    if (Int32.TryParse(searchingFilter.SearchingValue, out searchValue))
                    {
                        if (searchingFilter.SearchingName.Contains(WKSADBConstants.RankId))
                        {
                            tournamentDivisions = tournamentDivisions.Where(m => m.Division.RankId == searchValue);
                        }
                        if (searchingFilter.SearchingName.Contains(WKSADBConstants.DivisionTypeId))
                        {
                            tournamentDivisions = tournamentDivisions.Where(m => m.Division.DivisionTypeId == searchValue);
                        }
                        if (searchingFilter.SearchingName.Contains(WKSADBConstants.AgeGroupId))
                        {
                            tournamentDivisions = tournamentDivisions.Where(m => m.Division.AgeGroupId == searchValue);
                        }
                    }
                }
            }

            int totalRecords = tournamentDivisions.Count();

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
            foreach (TournamentDivision tournamentDivision in tournamentDivisions.OrderBy(request.SortingName + " " + request.SortingOrder.ToString()).Skip(request.PageIndex * request.RecordsCount).Take(request.PagesCount.HasValue ? request.PagesCount.Value : 1 * request.RecordsCount))
            {
                response.Records.Add(new JqGridRecord(Convert.ToString(tournamentDivision.DivisionId), new List<object>()
                {
                    tournamentDivision.DivisionId,
                    tournamentDivision.Division.DivisionName,
                    tournamentDivision.Division.Rank.Description,
                    tournamentDivision.Division.DivisionType.Description,
                    tournamentDivision.Division.AgeGroup.Description
                }));
            }

            //Return data as json
            return new JqGridJsonResult() { Data = response };
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }

        private void InitViewBag(TournamentDivision tournamentdivision)
        {
            ViewBag.Competitors = db.CompetitorDivisions.Where(m => m.DivisionId == tournamentdivision.DivisionId && m.Competitor.TournamentId == tournamentdivision.TournamentId);
            ViewBag.DivisionId = new SelectList(db.Divisions, "DivisionId", "DivisionName", tournamentdivision.DivisionId);
            ViewBag.TournamentId = new SelectList(db.Tournaments, "TournamentId", "TournamentName", tournamentdivision.TournamentId);

            GetTournamentStats(tournamentdivision.Tournament);
        }
    }
}