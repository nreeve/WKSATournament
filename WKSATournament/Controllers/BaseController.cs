using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WKSATournament.Models;
using WKSADB;

namespace WKSATournament.Controllers
{
    public class BaseController : Controller
    {
        internal ActionResult ContextDependentView()
        {
            string actionName = ControllerContext.RouteData.GetRequiredString("action");
            if (Request.QueryString["content"] != null)
            {
                ViewBag.FormAction = "Json" + actionName;
                return PartialView();
            }
            else
            {
                ViewBag.FormAction = actionName;
                return View();
            }
        }

        internal IEnumerable<string> GetErrorsFromModelState()
        {
            return ModelState.SelectMany(x => x.Value.Errors.Select(error => error.ErrorMessage));
        }

        internal void GetTournamentStats(Tournament tournament)
        {
            TournamentStats tournamentStats = new TournamentStats
            {
                TournamentId = tournament.TournamentId,
                TournamentName = tournament.TournamentName,
                TournamentDetails = string.Format("{0} ({1} - {2})", tournament.Venue.VenueName, tournament.StartDate.ToShortDateString(), tournament.EndDate.ToShortDateString()),
                TotalCompetitors = tournament.Competitors.Count,
                TotalDivisions = tournament.TournamentDivisions.Count,
                CompletedDivisions = tournament.TournamentDivisions.Count(m => m.IsCompleted),
                MedalsAwarded = tournament.TournamentDivisions.Count(m => m.MedalsReceived)
            };

            ViewBag.TournamentStats = tournamentStats;
        }
    }
}
