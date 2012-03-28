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
    public class SchoolController : Controller
    {
        private WKSAEntities db = new WKSAEntities();

        //
        // GET: /School/

        public ActionResult Index()
        {
            var schools = db.Schools.Include("Country").Include("Student");
            return View(schools.ToList());
        }

        //
        // GET: /School/Details/5

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

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}