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
    public class RankController : Controller
    {
        private WKSAEntities db = new WKSAEntities();

        //
        // GET: /Rank/

        public ActionResult Index()
        {
            return View(db.Ranks.ToList());
        }

        //
        // GET: /Rank/Details/5

        public ActionResult Details(int id = 0)
        {
            Rank rank = db.Ranks.Single(r => r.RankId == id);
            if (rank == null)
            {
                return HttpNotFound();
            }
            return View(rank);
        }

        //
        // GET: /Rank/Create

        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /Rank/Create

        [HttpPost]
        public ActionResult Create(Rank rank)
        {
            if (ModelState.IsValid)
            {
                db.Ranks.AddObject(rank);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(rank);
        }

        //
        // GET: /Rank/Edit/5

        public ActionResult Edit(int id = 0)
        {
            Rank rank = db.Ranks.Single(r => r.RankId == id);
            if (rank == null)
            {
                return HttpNotFound();
            }
            return View(rank);
        }

        //
        // POST: /Rank/Edit/5

        [HttpPost]
        public ActionResult Edit(Rank rank)
        {
            if (ModelState.IsValid)
            {
                db.Ranks.Attach(rank);
                db.ObjectStateManager.ChangeObjectState(rank, EntityState.Modified);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(rank);
        }

        //
        // GET: /Rank/Delete/5

        public ActionResult Delete(int id = 0)
        {
            Rank rank = db.Ranks.Single(r => r.RankId == id);
            if (rank == null)
            {
                return HttpNotFound();
            }
            return View(rank);
        }

        //
        // POST: /Rank/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            Rank rank = db.Ranks.Single(r => r.RankId == id);
            db.Ranks.DeleteObject(rank);
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