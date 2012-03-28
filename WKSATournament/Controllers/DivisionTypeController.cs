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
    public class DivisionTypeController : Controller
    {
        private WKSAEntities db = new WKSAEntities();

        //
        // GET: /DivisionType/

        public ActionResult Index()
        {
            return View(db.DivisionTypes.ToList());
        }

        //
        // GET: /DivisionType/Details/5

        public ActionResult Details(int id = 0)
        {
            DivisionType divisiontype = db.DivisionTypes.Single(d => d.DivisionTypeId == id);
            if (divisiontype == null)
            {
                return HttpNotFound();
            }
            return View(divisiontype);
        }

        //
        // GET: /DivisionType/Create

        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /DivisionType/Create

        [HttpPost]
        public ActionResult Create(DivisionType divisiontype)
        {
            if (ModelState.IsValid)
            {
                db.DivisionTypes.AddObject(divisiontype);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(divisiontype);
        }

        //
        // GET: /DivisionType/Edit/5

        public ActionResult Edit(int id = 0)
        {
            DivisionType divisiontype = db.DivisionTypes.Single(d => d.DivisionTypeId == id);
            if (divisiontype == null)
            {
                return HttpNotFound();
            }
            return View(divisiontype);
        }

        //
        // POST: /DivisionType/Edit/5

        [HttpPost]
        public ActionResult Edit(DivisionType divisiontype)
        {
            if (ModelState.IsValid)
            {
                db.DivisionTypes.Attach(divisiontype);
                db.ObjectStateManager.ChangeObjectState(divisiontype, EntityState.Modified);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(divisiontype);
        }

        //
        // GET: /DivisionType/Delete/5

        public ActionResult Delete(int id = 0)
        {
            DivisionType divisiontype = db.DivisionTypes.Single(d => d.DivisionTypeId == id);
            if (divisiontype == null)
            {
                return HttpNotFound();
            }
            return View(divisiontype);
        }

        //
        // POST: /DivisionType/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            DivisionType divisiontype = db.DivisionTypes.Single(d => d.DivisionTypeId == id);
            db.DivisionTypes.DeleteObject(divisiontype);
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