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
    }
    
    public class StudentController : Controller
    {
        private WKSAEntities db = new WKSAEntities();

        //
        // GET: /Student/

        public ActionResult Index()
        {
            var students = db.Students.Include("Country").Include("Rank").Include("School");
            return View(students.ToList());
        }

        //
        // GET: /Student/Details/5

        public ActionResult Details(int id = 0)
        {
            Student student = db.Students.Single(s => s.StudentId == id);
            if (student == null)
            {
                return HttpNotFound();
            }
            return View(student);
        }

        //
        // GET: /Student/Create

        public ActionResult Create()
        {
            ViewBag.CountryId = new SelectList(db.Countries, "CountryId", "CountryName");
            ViewBag.RankId = new SelectList(db.Ranks, "RankId", "Description");
            ViewBag.SchoolId = new SelectList(db.Schools, "SchoolId", "SchoolCode");
            return View();
        }

        //
        // POST: /Student/Create

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
                        DateOfBirth = student.DateOfBirth.HasValue ? student.DateOfBirth.Value.ToString("dd/MM/yyyy") : null
                    }).OrderBy(m => m.FirstName).ThenBy(m => m.LastName).ThenBy(m => m.SchoolName), JsonRequestBehavior.AllowGet);
            }
            else
            {
                return null;
            }
        }
        
        //
        // GET: /Student/Edit/5

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

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}