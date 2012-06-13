using System;
using WKSADB;

namespace WKSATournament.Models
{
    public class Address
    {
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string Address3 { get; set; }
        public string Address4 { get; set; }
        public string Address5 { get; set; }
        public string Postcode { get; set; }
        public int? CountryId { get; set; }

        public Address()
        {
        }

        public Address(Student student)
        {
            if (student != null)
            {
            }
        }
        
        public Address(Competitor competitor)
        {
            if (competitor != null)
            {
                if (competitor.ShareContactDetails && competitor.CompetitorDetail != null)
                {
                    Address1 = competitor.CompetitorDetail.Address1;
                    Address2 = competitor.CompetitorDetail.Address2;
                    Address3 = competitor.CompetitorDetail.Address3;
                    Address4 = competitor.CompetitorDetail.Address4;
                    Address5 = competitor.CompetitorDetail.Address5;
                    Postcode = competitor.CompetitorDetail.Postcode;
                    CountryId = competitor.CompetitorDetail.CountryId;
                }
                else
                {
                    Address1 = competitor.Student.Address1;
                    Address2 = competitor.Student.Address2;
                    Address3 = competitor.Student.Address3;
                    Address4 = competitor.Student.Address4;
                    Address5 = competitor.Student.Address5;
                    Postcode = competitor.Student.Postcode;
                    CountryId = competitor.Student.CountryId;
                }
            }
        }
    }
}
