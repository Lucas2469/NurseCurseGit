using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using System.IO;
using Microsoft.AspNetCore.Mvc;

namespace NurseCourse.Controllers
{
    public class PDFController : Controller
    {
        [HttpPost]
        public JsonResult GetPdfContent(string pdfPath)
        {
            string content = ReadPdfContent(pdfPath);
            if(ReadPdfContent(pdfPath).Length == 0) { }
            return Json(new { content });
        }

        private string ReadPdfContent(string pdfPath)
        {
            string text = string.Empty;

            using (var pdfReader = new PdfReader(pdfPath))
            using (var pdfDoc = new PdfDocument(pdfReader))
            {
                for (int page = 1; page <= pdfDoc.GetNumberOfPages(); page++)
                {
                    text += PdfTextExtractor.GetTextFromPage(pdfDoc.GetPage(page));
                }
            }

            return text;
        }
    }
}
