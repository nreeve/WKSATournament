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
    public class JsonStudent
    {
        public int StudentId { get; set; }
        public string WKSAId { get; set; }
        public string BlackBeltId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int RankId { get; set; }
        public int SchoolId { get; set; }
        public string SchoolCode { get; set; }
        public string SchoolName { get; set; }
        public string DateOfBirth { get; set; }
        public string Gender { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string Address3 { get; set; }
        public string Address4 { get; set; }
        public string Address5 { get; set; }
        public string Postcode { get; set; }
        public int? CountryId { get; set; }
    }

    public class StudentController : Controller
    {
        private WKSAEntities db = new WKSAEntities();

        public ActionResult Index()
        {
            ViewBag.RankFilter = db.Ranks.CreateJQGridFilter(WKSADBConstants.RankId, WKSADBConstants.Description);
            ViewBag.SchoolFilter = db.Schools.OrderBy(m => m.SchoolName).CreateJQGridFilter(WKSADBConstants.SchoolId, WKSADBConstants.SchoolName);

            return View();
        }

        public ActionResult Details(int id = 0)
        {
            Student student = db.Students.Single(s => s.StudentId == id);
            if (student == null)
            {
                return HttpNotFound();
            }
            return View(student);
        }

        public ActionResult Create()
        {
            ViewBag.CountryId = new SelectList(db.Countries, "CountryId", "CountryName");
            ViewBag.RankId = new SelectList(db.Ranks, "RankId", "Description");
            ViewBag.SchoolId = new SelectList(db.Schools, "SchoolId", "SchoolCode");
            return View();
        }

        [HttpPost]
        public ActionResult Create(Student student)
        {
            if (ModelState.IsValid)
            {
                db.Students.AddObject(student);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.CountryId = new SelectList(db.Countries, "CountryId", "CountryName", student.CountryId);
            ViewBag.RankId = new SelectList(db.Ranks, "RankId", "Description", student.RankId);
            ViewBag.SchoolId = new SelectList(db.Schools, "SchoolId", "SchoolCode", student.SchoolId);
            return View(student);
        }

        public JsonResult GetStudentByInfo(string WKSAId, string BlackBeltId, int? SchoolId, string FirstName, string LastName)
        {
            bool found = false;
            IEnumerable<Student> studentList = db.Students.AsEnumerable();
            
            if (!string.IsNullOrWhiteSpace(WKSAId))
            {
                studentList = studentList.Where(m => (m.WKSAId ?? string.Empty).Equals(WKSAId));
                found = true;
            }

            if (!string.IsNullOrWhiteSpace(BlackBeltId))
            {
                studentList = studentList.Where(m => (m.BlackBeltId ?? string.Empty).Equals(BlackBeltId));
                found = true;
            }

            if (SchoolId.HasValue)
            {
                studentList = studentList.Where(m => m.SchoolId == SchoolId.Value);
                found = true;
            }

            if (!string.IsNullOrWhiteSpace(FirstName))
            {
                studentList = studentList.Where(m => m.FirstName.ToLower().Contains(FirstName.ToLower()));
                found = true;
            }

            if (!string.IsNullOrWhiteSpace(LastName))
            {
                studentList = studentList.Where(m => m.LastName.ToLower().Contains(LastName.ToLower()));
                found = true;
            }

            if (found)
            {
                return Json(studentList.Select(student => new JsonStudent
                    {
                        StudentId = student.StudentId,
                        WKSAId = student.WKSAId,
                        BlackBeltId = student.BlackBeltId,
                        FirstName = student.FirstName,
                        LastName = student.LastName,
                        RankId  = student.RankId,
                        SchoolId = student.School.SchoolId,
                        SchoolCode = student.School.SchoolCode,
                        SchoolName = student.School.SchoolName,
                        Gender = student.Gender,
                        DateOfBirth = student.DateOfBirth.HasValue ? student.DateOfBirth.Value.ToString("dd/MM/yyyy") : null,
                        Address1 = student.Address1,
                        Address2 = student.Address2,
                        Address3 = student.Address3,
                        Address4 = student.Address4,
                        Address5 = student.Address5,
                        Postcode = student.Postcode,
                        CountryId = student.CountryId
                    }).OrderBy(m => m.FirstName).ThenBy(m => m.LastName).ThenBy(m => m.SchoolName), JsonRequestBehavior.AllowGet);
            }
            else
            {
                return null;
            }
        }
        
        //TODO: Create and Edit need the same view. Maybe could be used as a partial that could go into the Competitor page
        //TODO: Needs an option for 'Tournament History'
        public ActionResult Edit(int id = 0)
        {
            Student student = db.Students.Single(s => s.StudentId == id);
            if (student == null)
            {
                return HttpNotFound();
            }
            ViewBag.CountryId = new SelectList(db.Countries, "CountryId", "CountryName", student.CountryId);
            ViewBag.RankId = new SelectList(db.Ranks, "RankId", "Description", student.RankId);
            ViewBag.SchoolId = new SelectList(db.Schools, "SchoolId", "SchoolCode", student.SchoolId);
            return View(student);
        }

        //
        // POST: /Student/Edit/5

        [HttpPost]
        public ActionResult Edit(Student student)
        {
            if (ModelState.IsValid)
            {
                db.Students.Attach(student);
                db.ObjectStateManager.ChangeObjectState(student, EntityState.Modified);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.CountryId = new SelectList(db.Countries, "CountryId", "CountryName", student.CountryId);
            ViewBag.RankId = new SelectList(db.Ranks, "RankId", "Description", student.RankId);
            ViewBag.SchoolId = new SelectList(db.Schools, "SchoolId", "SchoolCode", student.SchoolId);
            return View(student);
        }

        //
        // GET: /Student/Delete/5

        public ActionResult Delete(int id = 0)
        {
            Student student = db.Students.Single(s => s.StudentId == id);
            if (student == null)
            {
                return HttpNotFound();
            }
            return View(student);
        }

        //
        // POST: /Student/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            Student student = db.Students.Single(s => s.StudentId == id);
            db.Students.DeleteObject(student);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult GridData(JqGridRequest request)
        {
            IQueryable<Student> students = db.Students.OrderBy(m => m.FirstName).ThenBy(m => m.LastName);

            if (request.Searching)
            {
                foreach (JqGridRequestSearchingFilter searchingFilter in request.SearchingFilters.Filters)
                {
                    // No idea why I have to assign this to a string var first. Doesn't work otherwise!?!
                    string searchText = searchingFilter.SearchingValue;

                    if (searchingFilter.SearchingName.Contains(WKSADBConstants.FirstName))
                    {
                        students = students.Where(m => m.FirstName.Contains(searchText));
                    }
                    else if (searchingFilter.SearchingName.Contains(WKSADBConstants.LastName))
                    {
                        students = students.Where(m => m.LastName.Contains(searchText));
                    }
                    else
                    {
                        int searchValue;

                        if (Int32.TryParse(searchingFilter.SearchingValue, out searchValue))
                        {
                            if (searchingFilter.SearchingName.Contains(WKSADBConstants.SchoolName))
                            {
                                students = students.Where(m => m.SchoolId == searchValue);
                            }
                            else if (searchingFilter.SearchingName.Contains(WKSADBConstants.RankId))
                            {
                                students = students.Where(m => m.RankId == searchValue);
                            }
                        }
                    }
                }
            }

            int totalRecords = students.Count();

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
            foreach (Student student in students.OrderBy(request.SortingName + " " + request.SortingOrder.ToString()).Skip(request.PageIndex * request.RecordsCount).Take(request.PagesCount.HasValue ? request.PagesCount.Value : 1 * request.RecordsCount))
            {
                response.Records.Add(new JqGridRecord(Convert.ToString(student.StudentId), new List<object>()
                {
                    student.StudentId,
                    student.WKSAId,
                    student.BlackBeltId,
                    student.FirstName,
                    student.LastName,
                    student.Rank.Description,
                    student.School.SchoolName
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