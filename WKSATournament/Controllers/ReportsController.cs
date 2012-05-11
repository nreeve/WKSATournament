using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;
using WKSADB;
using WKSATournament.Extensions;

namespace WKSATournament.Controllers
{
    public class ReportsController : Controller
    {
        private WKSAEntities db = new WKSAEntities();

        MemoryStream ms = new MemoryStream();
        Document document = new Document();
        PdfWriter PDFWriter = null;
        BaseFont baseFont = null;
        Font fontNormal = null;
        Font fontH1 = null;
        Font fontHeader = null;

        public FileStreamResult AttendByRank(int tournamentId)
        {
            InitDocument();

            document.Open();

            PdfPTable headerTable = new PdfPTable(2);
            headerTable.HorizontalAlignment = Element.ALIGN_LEFT;
            headerTable.DefaultCell.VerticalAlignment = Element.ALIGN_MIDDLE;
            headerTable.DefaultCell.Border = Rectangle.NO_BORDER;
            headerTable.WidthPercentage = 100f;
            headerTable.SetWidths(new float[] { 10, 90 });

            headerTable.AddCell(Image.GetInstance(Server.MapPath("/Content/images/reportLogo.gif")));
            headerTable.AddCell(new Phrase("Attendance By Rank", fontH1));

            document.Add(headerTable);

            Font fontRank = new Font(baseFont, 12, Font.BOLD);
            Font fontAgeGroup = new Font(baseFont, 10, Font.BOLD);

            IEnumerable<CompetitorDivision> competitorDivisions = db.CompetitorDivisions.Where(m => m.Competitor.TournamentId == tournamentId).OrderBy(m => m.Division.RankId).ThenBy(m => m.Division.AgeGroupId);

            foreach (IGrouping<int, CompetitorDivision> rankDivisions in competitorDivisions.ToLookup(m => m.Division.RankId))
            {
                PdfPTable rankTable = new PdfPTable(4);
                rankTable.SpacingBefore = 10f;
                rankTable.HorizontalAlignment = Element.ALIGN_LEFT;
                rankTable.WidthPercentage = 100f;
                rankTable.DefaultCell.Border = Rectangle.NO_BORDER;
                rankTable.SetWidths(new float[] { 70, 10, 10, 10 });

                Rank rank = rankDivisions.First().Division.Rank;

                rankTable.AddCell(new Phrase(rank.Description, fontRank));
                rankTable.AddCell(new Phrase("Total", fontRank));
                rankTable.AddCell(new Phrase("M", fontRank));
                rankTable.AddCell(new Phrase("F", fontRank));

                foreach (IGrouping<int, CompetitorDivision> ageGroupDivisions in rankDivisions.Where(m => !m.Division.AgeGroup.IsSparringGroup).ToLookup(m => m.Division.AgeGroupId))
                {
                    AgeGroup ageGroup = ageGroupDivisions.First().Division.AgeGroup;
                    PdfPCell ageGroupCell = iTextSharpHelper.CreateCell(ageGroup.Description, fontAgeGroup, 0, Element.ALIGN_LEFT, null);
                    ageGroupCell.Colspan = 4;
                    rankTable.AddCell(ageGroupCell);

                    fillAttendByRankTable(ageGroupDivisions.Where(m => !m.Division.AgeGroup.IsSparringGroup).ToLookup(m => m.DivisionId), rankTable);
                    
                    // Include child age groups for sparring
                    fillAttendByRankTable(db.CompetitorDivisions.Where(m => m.Competitor.TournamentId == tournamentId && m.Division.AgeGroup.ParentAgeGroupId == ageGroup.AgeGroupId).ToLookup(m => m.DivisionId), rankTable);
                }

                document.Add(rankTable);
            }

            document.Close();

            return getFileStreamResult(ms, "attendancebyrank.pdf");
        }

        public FileStreamResult CreateDivisionSheet(int tournamentId, int divisionId)
        {
            InitDocument();

            iTextSharpHeaderFooter pageEventHandler = new iTextSharpHeaderFooter();
            pageEventHandler.BaseFont = baseFont;
            pageEventHandler.ShowFooter = true;
            PDFWriter.PageEvent = pageEventHandler;

            document.Open();

            createEventSheet(tournamentId, db.TournamentDivisions.Single(m => m.TournamentId == tournamentId && m.DivisionId == divisionId));

            document.Close();

            return getFileStreamResult(ms, "tournamentsheet.pdf");
        }

        [HttpPost]
        public FileStreamResult CreateDivisionSheets(int id, int? RankId, int? DivisionTypeId, int? AgeGroupId, bool IncludeRankSheet, bool IncludeAgeGroupSheet)
        {
            InitDocument();

            iTextSharpHeaderFooter pageEventHandler = new iTextSharpHeaderFooter();
            pageEventHandler.BaseFont = baseFont;
            PDFWriter.PageEvent = pageEventHandler;

            document.Open();

            IEnumerable<TournamentDivision> tournamentDivisions = db.TournamentDivisions.Where(m => m.TournamentId == id);
            if(RankId.HasValue)
            {
                tournamentDivisions = tournamentDivisions.Where(m => m.Division.RankId == RankId.Value);
            }
            if(DivisionTypeId.HasValue)
            {
                tournamentDivisions = tournamentDivisions.Where(m => m.Division.DivisionTypeId == DivisionTypeId.Value);
            }
            if(AgeGroupId.HasValue)
            {
                tournamentDivisions = tournamentDivisions.Where(m => m.Division.AgeGroupId == AgeGroupId.Value || m.Division.AgeGroup.ParentAgeGroupId == AgeGroupId.Value);
            }

            IEnumerable<Rank> ranks = db.Ranks;

            foreach (IGrouping<int, TournamentDivision> rankDivisionList in tournamentDivisions.ToLookup(m => m.Division.RankId))
            {
                if (IncludeRankSheet)
                {
                    // Create Rank Insert Sheet
                    document.SetPageSize(PageSize.A4.Rotate());
                    document.NewPage();
                    Paragraph rankParagraph = new Paragraph(ranks.Single(m => m.RankId == rankDivisionList.Key).Description.ToUpper(), fontHeader);
                    rankParagraph.Alignment = Rectangle.ALIGN_RIGHT;
                    document.Add(rankParagraph);
                    pageEventHandler.ShowFooter = false;
                }

                foreach (IGrouping<int, TournamentDivision> ageGroupDivisionList in rankDivisionList.Where(m => !m.Division.AgeGroup.IsSparringGroup).ToLookup(m => m.Division.AgeGroupId))
                {
                    if (IncludeAgeGroupSheet)
                    {
                        // Create Age Group Insert Sheet
                        document.SetPageSize(PageSize.A4.Rotate());
                        document.NewPage();
                        Paragraph rankParagraph = new Paragraph(ranks.Single(m => m.RankId == rankDivisionList.Key).Description.ToUpper(), fontHeader);
                        rankParagraph.Alignment = Rectangle.ALIGN_RIGHT;
                        document.Add(rankParagraph);
                        Paragraph ageGroupParagraph = new Paragraph(db.AgeGroups.Single(m => m.AgeGroupId == ageGroupDivisionList.Key).Description, fontHeader);
                        ageGroupParagraph.Alignment = Rectangle.ALIGN_RIGHT;
                        document.Add(ageGroupParagraph);
                        pageEventHandler.ShowFooter = false;
                    }

                    foreach (TournamentDivision tournamentDivision in tournamentDivisions.Where(m => m.TournamentId == id && m.Division.RankId == rankDivisionList.Key && (m.Division.AgeGroupId == ageGroupDivisionList.Key || m.Division.AgeGroup.ParentAgeGroupId == ageGroupDivisionList.Key)))
                    {
                        document.SetPageSize(PageSize.A4);
                        document.NewPage();
                        pageEventHandler.ShowFooter = true;

                        createEventSheet(id, tournamentDivision);
                    }
                }
            }

            document.Close();

            return getFileStreamResult(ms, "tournamentsheets.pdf");
        }

        public FileStreamResult SchoolReport(int tournamentId, Constants.ReportType reportType)
        {
            InitDocument();
            
            document.Open();

            PdfPTable headerTable = new PdfPTable(2);
            headerTable.HorizontalAlignment = Element.ALIGN_LEFT;
            headerTable.DefaultCell.VerticalAlignment = Element.ALIGN_MIDDLE;
            headerTable.DefaultCell.Border = Rectangle.NO_BORDER;
            headerTable.WidthPercentage = 100f;
            headerTable.SetWidths(new float[] { 10, 90 });

            headerTable.AddCell(Image.GetInstance(Server.MapPath("/Content/images/reportLogo.gif")));

            string reportFilename = string.Empty;

            ILookup<int, Competitor> competitorList = db.Competitors.Where(m => m.TournamentId == tournamentId).ToLookup(m => m.Student.SchoolId);
            IOrderedEnumerable<IGrouping<int, Competitor>> schools = null;

            switch (reportType)
            {
                case Constants.ReportType.Financial:
                    headerTable.AddCell(new Phrase("Financial Report", fontH1));
                    reportFilename = "financial.pdf";
                    schools = competitorList.OrderByDescending(m => m.Sum(f => f.Fee));
                    break;
                case Constants.ReportType.SchoolAttend:
                    headerTable.AddCell(new Phrase("School Attendance Report", fontH1));
                    reportFilename = "schoolattendance.pdf";
                    schools = competitorList.OrderByDescending(m => m.Count());
                    break;
                case Constants.ReportType.SchoolPlaces:
                    headerTable.AddCell(new Phrase("School Places", fontH1));
                    reportFilename = "schoolplaces.pdf";
                    schools = competitorList.OrderByDescending(m => m.Sum(c => c.CompetitorDivisions.Sum(cd => cd.Result)));
                    break;
            }

            document.Add(headerTable);

            PdfPTable schoolTable = new PdfPTable(reportType == Constants.ReportType.Financial ? 5 : 4);
            schoolTable.SpacingBefore = 10f;
            schoolTable.HorizontalAlignment = Element.ALIGN_LEFT;
            schoolTable.DefaultCell.Border = Rectangle.NO_BORDER;
            schoolTable.WidthPercentage = 100f;

            Font fontTableHeader = new Font(baseFont, 10, Font.BOLD);
            schoolTable.AddCell(new Phrase("School Id", fontTableHeader));
            schoolTable.AddCell(new Phrase("School Name", fontTableHeader));
            schoolTable.AddCell(new Phrase("Instructor Name", fontTableHeader));

            if (reportType == Constants.ReportType.SchoolPlaces)
            {
                schoolTable.AddCell(new Phrase("Total Points", fontTableHeader));
            }
            else
            {
                schoolTable.AddCell(new Phrase("Total Attended", fontTableHeader));
            }

            if (reportType == Constants.ReportType.Financial)
            {
                schoolTable.AddCell(new Phrase("Total Fee", fontTableHeader));
            }

            foreach (IGrouping<int, Competitor> schoolCompetitors in schools)
            {
                School schoolItem = schoolCompetitors.First().Student.School;

                schoolTable.AddCell(new Phrase(schoolItem.SchoolCode, fontNormal));
                schoolTable.AddCell(new Phrase(schoolItem.SchoolName, fontNormal));
                schoolTable.AddCell(new Phrase(schoolItem.InstructorName, fontNormal));

                if (reportType == Constants.ReportType.SchoolPlaces)
                {
                    schoolTable.AddCell(new Phrase(schoolCompetitors.Sum(c => c.CompetitorDivisions.Sum(cd => cd.Result)).ToString(), fontNormal));
                }
                else
                {
                    schoolTable.AddCell(new Phrase(schoolCompetitors.Count().ToString(), fontNormal));
                }

                if (reportType == Constants.ReportType.Financial)
                {
                    schoolTable.AddCell(new Phrase(schoolCompetitors.Sum(m => m.Fee).ToString("c"), fontNormal));
                }
            }

            document.Add(schoolTable);

            document.Close();

            return getFileStreamResult(ms, reportFilename);
        }

        private void createEventSheet(int tournamentId, TournamentDivision tournamentDivision)
        {
            List<CompetitorDivision> competitorDivisions = db.CompetitorDivisions.Where(m => m.Competitor.TournamentId == tournamentId && m.DivisionId == tournamentDivision.DivisionId).ToList();

            PdfPTable headerTable = new PdfPTable(4);
            headerTable.HorizontalAlignment = Element.ALIGN_LEFT;
            headerTable.DefaultCell.VerticalAlignment = Element.ALIGN_MIDDLE;
            headerTable.DefaultCell.Border = Rectangle.NO_BORDER;
            headerTable.WidthPercentage = 100f;
            headerTable.SetWidths(new float[] { 10, 60, 15, 15 });

            headerTable.AddCell(Image.GetInstance(Server.MapPath("/Content/images/reportLogo.gif")));
            headerTable.AddCell(new Phrase(tournamentDivision.Division.DivisionName, fontH1));
            headerTable.AddCell("Ring No.:");
            //TODO: Display event number on report
            PdfContentByte pdfContentByte = PDFWriter.DirectContent;
            Barcode128 code128 = new Barcode128 { Code = string.Format("*{0}*", tournamentDivision.DivisionId), Extended = false, CodeType = Barcode.CODE128, Font = null };
            Image image128 = code128.CreateImageWithBarcode(pdfContentByte, null, null);
            headerTable.AddCell(code128.CreateImageWithBarcode(pdfContentByte, null, null));

            document.Add(headerTable);
            //document.Add(new Paragraph("Please ensure that you complete this form (including the placings section at the bottom)"));

            //TODO: Sparring specific sheet needs bye sheet at the top
            if (tournamentDivision.Division.DivisionTypeId == WKSADBConstants.DivisionType_SparringId)
            {
                PdfPTable table = new PdfPTable(5);
                table.HorizontalAlignment = Element.ALIGN_LEFT;
                table.DefaultCell.Border = Rectangle.NO_BORDER;
                table.WidthPercentage = 100f;
                table.SpacingBefore = 5f;
                table.SpacingAfter = 5f;

                PdfPCell cell = iTextSharpHelper.CreateCell(string.Format("Competitor List (division requires {0} byes)", getByeCount(competitorDivisions.Count)), new Font(baseFont, 9, Font.BOLD), 0, Element.ALIGN_LEFT, null);
                cell.Colspan = 5;
                table.AddCell(cell);

                foreach (CompetitorDivision competitorDivision in competitorDivisions)
                {
                    table.AddCell(iTextSharpHelper.CreateCell(string.Format("{0} {1} ({2})", competitorDivision.Competitor.Student.FirstName, competitorDivision.Competitor.Student.LastName, competitorDivision.Competitor.Student.School.SchoolName), new Font(baseFont, 6), 0, Element.ALIGN_LEFT, null));
                }

                int remaining = competitorDivisions.Count % 5;
                for (int i = 0; i < 5 - remaining; i++)
                {
                    table.AddCell("");
                }

                document.Add(table);

                Image sparringTreeImage = Image.GetInstance(Server.MapPath("/Content/images/sparringtreev02.jpg"));
                // Have to scale as it's in 150dpi
                sparringTreeImage.ScalePercent(48f);
                document.Add(sparringTreeImage);
            }
            else
            {
                PdfPTable table = new PdfPTable(13);
                table.HorizontalAlignment = Element.ALIGN_LEFT;
                table.DefaultCell.Border = Rectangle.BOTTOM_BORDER;
                table.DefaultCell.PaddingTop = 10f;
                table.WidthPercentage = 100f;
                table.SetWidths(new float[] { 30, 2, 8, 2, 8, 2, 8, 2, 8, 2, 8, 2, 8 });
                table.SpacingBefore = 20f;

                PdfPCell gapCell = new PdfPCell();
                gapCell.Rowspan = competitorDivisions.Count() + 1;
                gapCell.Border = 0;

                table.AddCell(iTextSharpHelper.CreateCell(string.Empty, new Font(baseFont, 9), 0, Element.ALIGN_CENTER, null));
                table.AddCell(gapCell);
                table.AddCell(iTextSharpHelper.CreateCell("Judge 1", new Font(baseFont, 9), 0, Element.ALIGN_CENTER, null));
                table.AddCell(gapCell);
                table.AddCell(iTextSharpHelper.CreateCell("Judge 2", new Font(baseFont, 9), 0, Element.ALIGN_CENTER, null));
                table.AddCell(gapCell);
                table.AddCell(iTextSharpHelper.CreateCell("Judge 3", new Font(baseFont, 9), 0, Element.ALIGN_CENTER, null));
                table.AddCell(gapCell);
                table.AddCell(iTextSharpHelper.CreateCell("Judge 4", new Font(baseFont, 9), 0, Element.ALIGN_CENTER, null));
                table.AddCell(gapCell);
                table.AddCell(iTextSharpHelper.CreateCell("Judge 5", new Font(baseFont, 9), 0, Element.ALIGN_CENTER, null));
                table.AddCell(gapCell);
                table.AddCell(iTextSharpHelper.CreateCell("Total", new Font(baseFont, 9, Font.BOLD), 0, Element.ALIGN_CENTER, null));

                // Randomise Competitors
                competitorDivisions.Shuffle();
                foreach (CompetitorDivision competitorDivision in competitorDivisions)
                {
                    table.AddCell(string.Format("{0} {1}", competitorDivision.Competitor.Student.FirstName, competitorDivision.Competitor.Student.LastName));
                    table.AddCell(string.Empty);
                    table.AddCell(string.Empty);
                    table.AddCell(string.Empty);
                    table.AddCell(string.Empty);
                    table.AddCell(string.Empty);
                    table.AddCell(string.Empty);
                }

                document.Add(table);
            }
        }

        private void fillAttendByRankTable(ILookup<int, CompetitorDivision> competitorDivisions, PdfPTable table)
        {
            foreach (IGrouping<int, CompetitorDivision> division in competitorDivisions)
            {
                table.AddCell(new Phrase(division.First().Division.DivisionName, fontNormal));
                table.AddCell(new Phrase(division.Count().ToString(), fontNormal));
                table.AddCell(new Phrase(division.Count(m => m.Competitor.Student.Gender.Equals("M")).ToString(), fontNormal));
                table.AddCell(new Phrase(division.Count(m => m.Competitor.Student.Gender.Equals("F")).ToString(), fontNormal));
            }
        }

        private int getByeCount(int competitorCount)
        {
            /*3 = 1
            4 = 0
            5 = 3
            6 = 2
            7 = 1
            8 = 0
            9 = 7
            10 = 6
            11 = 5
            12 = 4
            13 = 3
            14 = 2
            15 = 1
            16 = 0
            17 = 15
            18 = 14*/
            if (competitorCount != 0)
            {
                List<int> power2 = new List<int>();

                power2.Add(128);
                power2.Add(64);
                power2.Add(32);
                power2.Add(16);
                power2.Add(8);
                power2.Add(4);
                power2.Add(2);

                return competitorCount - power2.First(m => m < competitorCount);
            }
            else
            {
                return 0;
            }
        }

        private FileStreamResult getFileStreamResult(MemoryStream ms, string filename)
        {
            Response.ContentType = "application/pdf";

            Response.AddHeader("content-disposition", "attachment;filename=" + filename);
            Response.Buffer = true;
            Response.Clear();
            Response.OutputStream.Write(ms.GetBuffer(), 0, ms.GetBuffer().Length);
            Response.OutputStream.Flush();
            Response.End();

            return new FileStreamResult(Response.OutputStream, "application/pdf");
        }

        private void InitDocument()
        {
            document.SetPageSize(PageSize.A4);

            PDFWriter = PdfWriter.GetInstance(document, ms);
            PDFWriter.ViewerPreferences = PdfWriter.PageLayoutSinglePage;

            // Set up font
            baseFont = BaseFont.CreateFont(BaseFont.HELVETICA,
                                                       BaseFont.CP1252,
                                                       BaseFont.NOT_EMBEDDED);

            fontNormal = new Font(baseFont, 10, Font.NORMAL);
            fontH1 = new Font(baseFont, 16, Font.BOLD);
            fontHeader = new Font(baseFont, 22, Font.BOLD);
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}
