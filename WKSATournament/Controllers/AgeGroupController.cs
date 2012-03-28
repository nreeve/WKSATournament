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
    public class AgeGroupController : Controller
    {
        private WKSAEntities db = new WKSAEntities();

        //
        // GET: /AgeGroup/

        public ActionResult Index()
        {
            return View(db.AgeGroups.OrderBy(m => m.FromAge ?? m.ToAge).ThenBy(m => m.ToAge ?? m.FromAge).ToList());
        }

        //
        // GET: /AgeGroup/Details/5

        public ActionResult Details(int id = 0)
        {
            AgeGroup agegroup = db.AgeGroups.Single(a => a.AgeGroupId == id);
            if (agegroup == null)
            {
                return HttpNotFound();
            }
            return View(agegroup);
        }

        //
        // GET: /AgeGroup/Create

        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /AgeGroup/Create

        [HttpPost]
        public ActionResult Create(AgeGroup agegroup)
        {
            if (ModelState.IsValid)
            {
                db.AgeGroups.AddObject(agegroup);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(agegroup);
        }

        //
        // GET: /AgeGroup/Edit/5

        public ActionResult Edit(int id = 0)
        {
            AgeGroup agegroup = db.AgeGroups.Single(a => a.AgeGroupId == id);
            if (agegroup == null)
            {
                return HttpNotFound();
            }
            return View(agegroup);
        }

        //
        // POST: /AgeGroup/Edit/5

        [HttpPost]
        public ActionResult Edit(AgeGroup agegroup)
        {
            if (ModelState.IsValid)
            {
                db.AgeGroups.Attach(agegroup);
                db.ObjectStateManager.ChangeObjectState(agegroup, EntityState.Modified);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(agegroup);
        }

        //
        // GET: /AgeGroup/Delete/5

        public ActionResult Delete(int id = 0)
        {
            AgeGroup agegroup = db.AgeGroups.Single(a => a.AgeGroupId == id);
            if (agegroup == null)
            {
                return HttpNotFound();
            }
            return View(agegroup);
        }

        //
        // POST: /AgeGroup/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            AgeGroup agegroup = db.AgeGroups.Single(a => a.AgeGroupId == id);
            db.AgeGroups.DeleteObject(agegroup);
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