using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using iTextSharp.text.pdf;
using iTextSharp.text;

namespace WKSATournament.Extensions
{
    public static class iTextSharpHelper
    {
        public static PdfPCell CreateCell(string cellText, Font font, int border, int horizontalAligntment, float? paddingLeft)
        {
            PdfPCell cell = new PdfPCell(new Phrase(cellText, font));
            cell.Border = border;
            cell.HorizontalAlignment = horizontalAligntment;
            
            if (paddingLeft.HasValue)
            {
                cell.PaddingLeft = paddingLeft.Value;
            }

            return cell;
        }
    }

    public class iTextSharpHeaderFooter :PdfPageEventHelper
    {
        // This is the contentbyte object of the writer
        PdfContentByte cb;

        public BaseFont BaseFont { get; set; }
        public bool ShowFooter { get; set; }

        // we override the onOpenDocument method
        public override void OnOpenDocument(PdfWriter writer, Document document)
        {
            try
            {
                cb = writer.DirectContent;
            }
            catch (DocumentException de)
            {
            }
            catch (System.IO.IOException ioe)
            {
            }
        }
        
        public override void OnEndPage(PdfWriter writer, Document document)
        {
            base.OnEndPage(writer, document);

            Font FooterFont = new Font(BaseFont, 10);

            if (ShowFooter)
            {
                PdfPTable placeTable = new PdfPTable(8);
                placeTable.HorizontalAlignment = Element.ALIGN_LEFT;
                placeTable.TotalWidth = document.PageSize.Width - 100;
                placeTable.SetWidths(new float[] { 7.5f, 16.5f, 9.5f, 16.5f, 8.5f, 16.5f, 8.5f, 16.5f });

                /*PdfPCell titleCell = iTextSharpHelper.CreateCell("Please ensure that you complete this form (including the placings section at the bottom)", new Font(BaseFont, 10, Font.ITALIC), Rectangle.NO_BORDER, Element.ALIGN_LEFT, null);
                titleCell.Colspan = 8;
                placeTable.AddCell(titleCell);*/

                PdfPCell placeCell = iTextSharpHelper.CreateCell(string.Empty, FooterFont, Rectangle.BOTTOM_BORDER, Element.ALIGN_LEFT, null);

                placeTable.AddCell(iTextSharpHelper.CreateCell("First", FooterFont, 0, Element.ALIGN_LEFT, null));
                placeTable.AddCell(placeCell);
                placeTable.AddCell(iTextSharpHelper.CreateCell("Second", FooterFont, 0, Element.ALIGN_LEFT, 10f));
                placeTable.AddCell(placeCell);
                placeTable.AddCell(iTextSharpHelper.CreateCell("Third", FooterFont, 0, Element.ALIGN_LEFT, 10f));
                placeTable.AddCell(placeCell);
                placeTable.AddCell(iTextSharpHelper.CreateCell("Fourth", FooterFont, 0, Element.ALIGN_LEFT, 10f));
                placeTable.AddCell(placeCell);

                placeTable.WriteSelectedRows(0, 1, document.PageSize.GetLeft(40), document.PageSize.GetBottom(90), cb);

                PdfPTable judgeTable = new PdfPTable(4);
                judgeTable.HorizontalAlignment = Element.ALIGN_LEFT;
                judgeTable.TotalWidth = document.PageSize.Width - 100;
                judgeTable.SetWidths(new float[] { 20f, 40f, 20f, 40f });

                judgeTable.AddCell(iTextSharpHelper.CreateCell("Center Judge", new Font(BaseFont, 11), 0, Element.ALIGN_LEFT, null));
                judgeTable.AddCell(placeCell);
                judgeTable.AddCell(iTextSharpHelper.CreateCell("Scorekeeper", new Font(BaseFont, 11), 0, Element.ALIGN_LEFT, 10f));
                judgeTable.AddCell(placeCell);

                judgeTable.WriteSelectedRows(0, 1, document.PageSize.GetLeft(40), document.PageSize.GetBottom(60), cb);
            }
        }
    }
}