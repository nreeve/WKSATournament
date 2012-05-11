using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WKSATournament.Models
{
    public class TournamentStats
    {
        public int CompletedDivisions { get; set; }
        public int MedalsAwarded { get; set; }
        public int MedalsRequired { get { return TotalDivisions - MedalsAwarded; } }
        public int TotalCompetitors { get; set; }
        public int TotalDivisions { get; set; }
        public int TournamentId { get; set; }
        public string TournamentName { get; set; }
        public string TournamentDetails { get; set; }
    }
}