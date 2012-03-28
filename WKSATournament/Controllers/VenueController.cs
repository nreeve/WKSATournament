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
    public class VenueController : BaseController
    {
        private WKSAEntities db = new WKSAEntities();

        //
        // GET: /Venue/

        public ActionResult Index()
        {
            var venues = db.Venues.Include("Country");
            return View(venues.ToList());
        }

        //
        // GET: /Venue/Details/5

        public ActionResult Details(int id = 0)
        {
            Venue venue = db.Venues.Single(v => v.VenueId == id);
            if (venue == null)
            {
                return HttpNotFound();
            }
            return View(venue);
        }

        public ActionResult Create()
        {
            ViewBag.CountryId = new SelectList(db.Countries, "CountryId", "CountryName");

            return ContextDependentView();
        }

        [HttpPost]
        public JsonResult JsonCreate(Venue venue)
        {
            if (ModelState.IsValid)
            {
                if (ModelState.IsValid)
                {
                    db.Venues.AddObject(venue);
                    db.SaveChanges();

                    return Json(new { success = true, returnData = new { venueId = venue.VenueId, venueName = venue.VenueName } });
                }
            }

            // If we got this far, something failed
            return Json(new { errors = GetErrorsFromModelState() });
        }
        
        //
        // POST: /Venue/Create

        [HttpPost]
        public ActionResult Create(Venue venue)
        {
            if (ModelState.IsValid)
            {
                db.Venues.AddObject(venue);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.CountryId = new SelectList(db.Countries, "CountryId", "CountryName", venue.CountryId);
            return View(venue);
        }

        //
        // GET: /Venue/Edit/5

        public ActionResult Edit(int id = 0)
        {
            Venue venue = db.Venues.Single(v => v.VenueId == id);
            if (venue == null)
            {
                return HttpNotFound();
            }
            ViewBag.CountryId = new SelectList(db.Countries, "CountryId", "CountryName", venue.CountryId);
            return View(venue);
        }

        //
        // POST: /Venue/Edit/5

        [HttpPost]
        public ActionResult Edit(Venue venue)
        {
            if (ModelState.IsValid)
            {
                db.Venues.Attach(venue);
                db.ObjectStateManager.ChangeObjectState(venue, EntityState.Modified);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.CountryId = new SelectList(db.Countries, "CountryId", "CountryName", venue.CountryId);
            return View(venue);
        }

        //
        // GET: /Venue/Delete/5

        public ActionResult Delete(int id = 0)
        {
            Venue venue = db.Venues.Single(v => v.VenueId == id);
            if (venue == null)
            {
                return HttpNotFound();
            }
            return View(venue);
        }

        //
        // POST: /Venue/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            Venue venue = db.Venues.Single(v => v.VenueId == id);
            db.Venues.DeleteObject(venue);
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