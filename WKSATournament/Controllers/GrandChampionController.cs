using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WKSADB;
using Lib.Web.Mvc.JQuery.JqGrid;

namespace WKSATournament.Controllers
{
    //TODO: Combine create and edit views
    public class GrandChampionController : Controller
    {
        private WKSAEntities db = new WKSAEntities();

        //
        // GET: /GrandChampion/

        public ActionResult Index()
        {
            var grandchampions = db.GrandChampions.Include("Rank");
            return View(grandchampions.ToList());
        }

        //
        // GET: /GrandChampion/Details/5

        public ActionResult Details(int id = 0)
        {
            GrandChampion grandchampion = db.GrandChampions.Single(g => g.GrandChampionId == id);
            if (grandchampion == null)
            {
                return HttpNotFound();
            }
            return View(grandchampion);
        }

        //
        // GET: /GrandChampion/Create

        public ActionResult Create()
        {
            ViewBag.AgeGroupId = new SelectList(db.AgeGroups, "AgeGroupId", "Description");
            ViewBag.RankId = new SelectList(db.Ranks, "RankId", "Description");
            ViewBag.Gender = new SelectList(new[]
            {
                new
                {
                    Name = "N/A",
                    Value = string.Empty
                },
                new
                {
                    Name = "Men's",
                    Value = "M"
                },
                new
                {
                    Name = "Women's",
                    Value = "F"
                }
            }, "Value", "Name");
            return View();
        }

        //
        // POST: /GrandChampion/Create

        [HttpPost]
        public ActionResult Create(GrandChampion grandchampion)
        {
            if (ModelState.IsValid)
            {
                db.GrandChampions.AddObject(grandchampion);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.RankId = new SelectList(db.Ranks, "RankId", "Description", grandchampion.RankId);
            return View(grandchampion);
        }

        //
        // GET: /GrandChampion/Edit/5

        public ActionResult Edit(int id = 0)
        {
            GrandChampion grandchampion = db.GrandChampions.Single(g => g.GrandChampionId == id);
            if (grandchampion == null)
            {
                return HttpNotFound();
            }
            ViewBag.RankId = new SelectList(db.Ranks, "RankId", "Description", grandchampion.RankId);
            return View(grandchampion);
        }

        //
        // POST: /GrandChampion/Edit/5

        [HttpPost]
        public ActionResult Edit(GrandChampion grandchampion)
        {
            if (ModelState.IsValid)
            {
                db.GrandChampions.Attach(grandchampion);
                db.ObjectStateManager.ChangeObjectState(grandchampion, EntityState.Modified);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.RankId = new SelectList(db.Ranks, "RankId", "Description", grandchampion.RankId);
            return View(grandchampion);
        }

        //
        // GET: /GrandChampion/Delete/5

        public ActionResult Delete(int id = 0)
        {
            GrandChampion grandchampion = db.GrandChampions.Single(g => g.GrandChampionId == id);
            if (grandchampion == null)
            {
                return HttpNotFound();
            }
            return View(grandchampion);
        }

        //
        // POST: /GrandChampion/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            GrandChampion grandchampion = db.GrandChampions.Single(g => g.GrandChampionId == id);
            db.GrandChampions.DeleteObject(grandchampion);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult GridData(JqGridRequest request, int id, int GrandChampionId)
        {
            IQueryable<Competitor> competitors = db.Competitors.Where(m => m.TournamentId == id && m.GrandChampionId == GrandChampionId).OrderByDescending(m => m.CompetitorDivisions.Sum(cd => cd.Result));

            int totalRecords = competitors.Count();

            //Prepare JqGridData instance
            JqGridResponse response = new JqGridResponse
            {
                //Total pages count
                TotalPagesCount = (int)Math.Ceiling((float)totalRecords / (float)request.RecordsCount),
                //Page number
                PageIndex = request.PageIndex,
                //Total records count
                TotalRecordsCount = totalRecords
            };

            //Table with rows data
            foreach (Competitor competitor in competitors.Skip(request.PageIndex * request.RecordsCount).Take(request.PagesCount.HasValue ? request.PagesCount.Value : 1 * request.RecordsCount))
            {
                string GoldCount = competitor.CompetitorDivisions.Count(m => m.Result == 5).ToString();
                string SilverCount = competitor.CompetitorDivisions.Count(m => m.Result == 3).ToString();
                string BronzeCount = competitor.CompetitorDivisions.Count(m => m.Result == 2).ToString();
                string CopperCount = competitor.CompetitorDivisions.Count(m => m.Result == 1).ToString();
                string TotalPoints = competitor.CompetitorDivisions.Sum(m => m.Result).ToString();
                response.Records.Add(new JqGridRecord(Convert.ToString(competitor.CompetitorId), new List<object>()
                {
                    competitor.CompetitorId,
                    competitor.Student.FirstName,
                    competitor.Student.LastName,
                    competitor.Student.School.SchoolName,
                    GoldCount,
                    SilverCount,
                    BronzeCount,
                    CopperCount,
                    TotalPoints
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
    }
}