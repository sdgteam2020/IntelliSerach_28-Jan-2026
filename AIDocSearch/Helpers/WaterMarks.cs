using iText.IO.Font.Constants;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas;
using iText.Kernel.Pdf.Extgstate;

namespace AIDocSearch.Helpers
{
    public static class WaterMarks
    {
        public static byte[] AddPdfWatermark(byte[] pdfBytes, string watermarkText)
        {
            //string ipAddress = "192.168.10.40";

            string dateTime =
                DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss");


            string[] lines =
            {
        watermarkText,
        dateTime
    };


            using var input = new MemoryStream(pdfBytes);
            using var output = new MemoryStream();


            using (var pdfReader = new PdfReader(input))
            using (var pdfWriter = new PdfWriter(output))
            using (var pdfDoc = new PdfDocument(pdfReader, pdfWriter))
            {
                var font = PdfFontFactory.CreateFont(
                    StandardFonts.HELVETICA_BOLD);


                int pages = pdfDoc.GetNumberOfPages();


                for (int i = 1; i <= pages; i++)
                {
                    var page = pdfDoc.GetPage(i);

                    var canvas = new PdfCanvas(page);

                    var pageSize = page.GetPageSize();


                    float x = pageSize.GetWidth() / 2;
                    float y = pageSize.GetHeight() / 2;


                    canvas.SaveState();


                    canvas.SetFillColor(
                        ColorConstants.RED);


                    canvas.SetFontAndSize(
                        font,
                        60);


                    canvas.ConcatMatrix(
                        0.7,
                        0.7,
                        -0.7,
                        0.7,
                        x,
                        y);


                    canvas.BeginText();


                    canvas.SetExtGState(
                        new PdfExtGState()
                            .SetFillOpacity(0.15f));


                    // First line position
                    canvas.MoveText(
                        -250,
                        100);


                    foreach (var line in lines)
                    {
                        canvas.ShowText(line);

                        // Move down for next line
                        canvas.MoveText(
                            0,
                            -120);
                    }


                    canvas.EndText();


                    canvas.RestoreState();
                }
            }


            return output.ToArray();
        }
    }
}
