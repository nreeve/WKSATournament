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
    public class JsonTournamentDivision
    {
        public string ErrorMessage { get; set; }
        public int DivisionId { get; set; }
        public string DivisionName { get; set; }
    }

    public class DivisionController : Controller
    {
        private WKSAEntities db = new WKSAEntities();

        //
        // GET: /Division/

        public ActionResult Index()
        {
            var divisions = db.Divisions.Include("AgeGroup").Include("DivisionType").Include("Rank");
            return View(divisions.OrderBy(m => m.RankId).ThenBy(m => m.DivisionTypeId).ThenBy(m => m.AgeGroup.FromAge).ThenBy(m => m.AgeGroup.ToAge).ToList());
        }

        //
        // GET: /Division/Details/5

        public ActionResult Details(int id = 0)
        {
            Division division = db.Divisions.Single(d => d.DivisionId == id);
            if (division == null)
            {
                return HttpNotFound();
            }
            return View(division);
        }

        //
        // GET: /Division/Create

        public ActionResult Create()
        {
            ViewBag.AgeGroupId = new SelectList(db.AgeGroups.OrderBy(m => m.FromAge ?? m.ToAge).ThenBy(m => m.ToAge ?? m.FromAge), "AgeGroupId", "Description");
            ViewBag.DivisionTypeId = new SelectList(db.DivisionTypes, "DivisionTypeId", "Description");
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
        // POST: /Division/Create

        [HttpPost]
        public ActionResult Create(Division division)
        {
            if (ModelState.IsValid)
            {
                db.Divisions.AddObject(division);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.AgeGroupId = new SelectList(db.AgeGroups.OrderBy(m => m.FromAge).ThenBy(m => m.ToAge), "AgeGroupId", "Description", division.AgeGroupId);
            ViewBag.DivisionTypeId = new SelectList(db.DivisionTypes, "DivisionTypeId", "Description", division.DivisionTypeId);
            ViewBag.RankId = new SelectList(db.Ranks, "RankId", "Description", division.RankId);
            return View(division);
        }

        public JsonResult GetDivision(int tournamentId, int divisionTypeId, int age, int rankId, string gender)
        {
            JsonTournamentDivision jsonTournamentDivision = new JsonTournamentDivision();

            if (db.TournamentDivisions.Any(m => m.TournamentId == tournamentId && m.Division.DivisionTypeId == divisionTypeId && m.Division.RankId == rankId && ((!m.Division.AgeGroup.FromAge.HasValue || m.Division.AgeGroup.FromAge.Value <= age) && (!m.Division.AgeGroup.ToAge.HasValue || m.Division.AgeGroup.ToAge.Value >= age)) && (string.IsNullOrEmpty(m.Division.Gender) || m.Division.Gender.Equals(gender))))
            {
                TournamentDivision tournamentDivision = db.TournamentDivisions.Single(m => m.TournamentId == tournamentId && m.Division.DivisionTypeId == divisionTypeId && m.Division.RankId == rankId && ((!m.Division.AgeGroup.FromAge.HasValue || m.Division.AgeGroup.FromAge <= age) && (!m.Division.AgeGroup.ToAge.HasValue || m.Division.AgeGroup.ToAge >= age)) && (string.IsNullOrEmpty(m.Division.Gender) || m.Division.Gender.Equals(gender)));

                jsonTournamentDivision.DivisionId = tournamentDivision.DivisionId;
                jsonTournamentDivision.DivisionName = tournamentDivision.Division.DivisionName;
            }
            else
            {
                jsonTournamentDivision.ErrorMessage = "No Divisions Found For This Rank/Age/Gender Combo";
            }

            return Json(jsonTournamentDivision, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Edit(int id = 0)
        {
            Division division = db.Divisions.Single(d => d.DivisionId == id);
            if (division == null)
            {
                return HttpNotFound();
            }
            ViewBag.AgeGroupId = new SelectList(db.AgeGroups.OrderBy(m => m.FromAge).ThenBy(m => m.ToAge), "AgeGroupId", "Description", division.AgeGroupId);
            ViewBag.DivisionTypeId = new SelectList(db.DivisionTypes, "DivisionTypeId", "Description", division.DivisionTypeId);
            ViewBag.RankId = new SelectList(db.Ranks, "RankId", "Description", division.RankId);
            return View(division);
        }

        //TODO: Add total to results and make it searchable with >, <
        [HttpPost]
        public ActionResult Edit(Division division)
        {
            if (ModelState.IsValid)
            {
                db.Divisions.Attach(division);
                db.ObjectStateManager.ChangeObjectState(division, EntityState.Modified);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.AgeGroupId = new SelectList(db.AgeGroups, "AgeGroupId", "AgeGroupId", division.AgeGroupId);
            ViewBag.DivisionTypeId = new SelectList(db.DivisionTypes, "DivisionTypeId", "Description", division.DivisionTypeId);
            ViewBag.RankId = new SelectList(db.Ranks, "RankId", "Description", division.RankId);
            return View(division);
        }

        //
        // GET: /Division/Delete/5

        public ActionResult Delete(int id = 0)
        {
            Division division = db.Divisions.Single(d => d.DivisionId == id);
            if (division == null)
            {
                return HttpNotFound();
            }
            return View(division);
        }

        //
        // POST: /Division/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            Division division = db.Divisions.Single(d => d.DivisionId == id);
            db.Divisions.DeleteObject(division);
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