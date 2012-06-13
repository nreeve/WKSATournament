using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WKSADB;
using WKSATournament.Extensions;
using Lib.Web.Mvc.JQuery.JqGrid;

namespace WKSATournament.Controllers
{
    public class SchoolController : Controller
    {
        private WKSAEntities db = new WKSAEntities();

        public ActionResult Index()
        {
            ViewBag.CountryFilter = db.Countries.OrderBy(m => m.CountryName).CreateJQGridFilter(WKSADBConstants.CountryId, WKSADBConstants.CountryName);

            return View();
        }

        //TODO: School home page that has student list, access to performance report etc
        public ActionResult Details(int id = 0)
        {
            School school = db.Schools.Single(s => s.SchoolId == id);
            if (school == null)
            {
                return HttpNotFound();
            }
            return View(school);
        }

        //
        // GET: /School/Create

        public ActionResult Create()
        {
            ViewBag.CountryId = new SelectList(db.Countries, "CountryId", "CountryName");
            ViewBag.InstructorId = new SelectList(db.Students, "StudentId", "WKSAId");
            return View();
        }

        //
        // POST: /School/Create

        [HttpPost]
        public ActionResult Create(School school)
        {
            if (ModelState.IsValid)
            {
                db.Schools.AddObject(school);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.CountryId = new SelectList(db.Countries, "CountryId", "CountryName", school.CountryId);
            ViewBag.InstructorId = new SelectList(db.Students, "StudentId", "WKSAId", school.InstructorId);
            return View(school);
        }

        //
        // GET: /School/Edit/5

        public ActionResult Edit(int id = 0)
        {
            School school = db.Schools.Single(s => s.SchoolId == id);
            if (school == null)
            {
                return HttpNotFound();
            }
            ViewBag.CountryId = new SelectList(db.Countries, "CountryId", "CountryName", school.CountryId);
            ViewBag.InstructorId = new SelectList(db.Students, "StudentId", "WKSAId", school.InstructorId);
            return View(school);
        }

        //
        // POST: /School/Edit/5

        [HttpPost]
        public ActionResult Edit(School school)
        {
            if (ModelState.IsValid)
            {
                db.Schools.Attach(school);
                db.ObjectStateManager.ChangeObjectState(school, EntityState.Modified);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.CountryId = new SelectList(db.Countries, "CountryId", "CountryName", school.CountryId);
            ViewBag.InstructorId = new SelectList(db.Students, "StudentId", "WKSAId", school.InstructorId);
            return View(school);
        }

        //
        // GET: /School/Delete/5

        public ActionResult Delete(int id = 0)
        {
            School school = db.Schools.Single(s => s.SchoolId == id);
            if (school == null)
            {
                return HttpNotFound();
            }
            return View(school);
        }

        //
        // POST: /School/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            School school = db.Schools.Single(s => s.SchoolId == id);
            db.Schools.DeleteObject(school);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult GridData(JqGridRequest request, int id)
        {
            IOrderedEnumerable<IGrouping<int, Competitor>> schools = null;
                
            switch(request.SortingName)
            {
                case "CompetitorCount":
                    if (request.SortingOrder.Equals(JqGridSortingOrders.Desc))
                    {
                        schools = db.Competitors.Where(m => m.TournamentId == id).ToLookup(m => m.Student.SchoolId).OrderBy(m => m.Count());
                    }
                    else
                    {
                        schools = db.Competitors.Where(m => m.TournamentId == id).ToLookup(m => m.Student.SchoolId).OrderByDescending(m => m.Count());
                    }
                    break;
                case "Average":
                    if (request.SortingOrder.Equals(JqGridSortingOrders.Desc))
                    {
                        schools = db.Competitors.Where(m => m.TournamentId == id).ToLookup(m => m.Student.SchoolId).OrderBy(m => m.Sum(c => c.CompetitorDivisions.Sum(cd => cd.Result)) / m.Count());
                    }
                    else
                    {
                        schools = db.Competitors.Where(m => m.TournamentId == id).ToLookup(m => m.Student.SchoolId).OrderByDescending(m => m.Sum(c => c.CompetitorDivisions.Sum(cd => cd.Result)) / m.Count());
                    }
                    break;
                case "TotalPoints":
                    if (request.SortingOrder.Equals(JqGridSortingOrders.Desc))
                    {
                        schools = db.Competitors.Where(m => m.TournamentId == id).ToLookup(m => m.Student.SchoolId).OrderByDescending(m => m.Sum(c => c.CompetitorDivisions.Sum(cd => cd.Result)));
                    }
                    else
                    {
                        schools = db.Competitors.Where(m => m.TournamentId == id).ToLookup(m => m.Student.SchoolId).OrderBy(m => m.Sum(c => c.CompetitorDivisions.Sum(cd => cd.Result)));
                    }
                    break;
            }

            int totalRecords = schools.Count();

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
            foreach (IGrouping<int, Competitor> schoolCompetitors in schools.Skip(request.PageIndex * request.RecordsCount).Take(request.PagesCount.HasValue ? request.PagesCount.Value : 1 * request.RecordsCount))
            {
                School school = schoolCompetitors.First().Student.School;
                int CompetitorCount = schoolCompetitors.Count();
                int? TotalPoints = schoolCompetitors.Sum(s => s.CompetitorDivisions.Sum(cd => cd.Result));
                decimal Average = TotalPoints.HasValue ? TotalPoints.Value / CompetitorCount : 0;

                response.Records.Add(new JqGridRecord(Convert.ToString(school.SchoolId), new List<object>()
                {
                    school.SchoolId,
                    school.SchoolCode,
                    school.SchoolName,
                    school.InstructorName,
                    CompetitorCount.ToString(),
                    Average.ToString("0.#"),
                    TotalPoints.ToString()
                }));
            }

            //Return data as json
            return new JqGridJsonResult() { Data = response };
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult HomePageGridData(JqGridRequest request)
        {
            IQueryable<School> schools = db.Schools.OrderBy(m => m.SchoolName);

            if (request.Searching)
            {
                foreach (JqGridRequestSearchingFilter searchingFilter in request.SearchingFilters.Filters)
                {
                    // No idea why I have to assign this to a string var first. Doesn't work otherwise!?!
                    string searchText = searchingFilter.SearchingValue;

                    if (searchingFilter.SearchingName.Contains(WKSADBConstants.SchoolCode))
                    {
                        schools = schools.Where(m => m.SchoolCode.Contains(searchText));
                    }
                    else if (searchingFilter.SearchingName.Contains(WKSADBConstants.SchoolName))
                    {
                        schools = schools.Where(m => m.SchoolName.Contains(searchText));
                    }
                    else if (searchingFilter.SearchingName.Contains(WKSADBConstants.InstructorName))
                    {
                        schools = schools.Where(m => m.InstructorName.Contains(searchText));
                    }
                    else
                    {
                        int searchValue;

                        if (Int32.TryParse(searchingFilter.SearchingValue, out searchValue))
                        {
                            if (searchingFilter.SearchingName.Contains(WKSADBConstants.CountryName))
                            {
                                schools = schools.Where(m => m.CountryId == searchValue);
                            }
                        }
                    }
                }
            }

            int totalRecords = schools.Count();

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
            foreach (School school in schools.OrderBy(request.SortingName + " " + request.SortingOrder.ToString()).Skip(request.PageIndex * request.RecordsCount).Take(request.PagesCount.HasValue ? request.PagesCount.Value : 1 * request.RecordsCount))
            {
                response.Records.Add(new JqGridRecord(Convert.ToString(school.SchoolId), new List<object>()
                {
                    school.SchoolId,
                    school.SchoolCode,
                    school.SchoolName,
                    school.Country.CountryName,
                    school.InstructorName
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