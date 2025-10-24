using System;
using System.IO;
using System.Windows.Ink;
using iText.Kernel.Pdf;
using iText.Kernel.Geom;
using iText.Kernel.Pdf.Canvas;

namespace DigitalSignClient.Services
{
    public class PdfSignService
    {
        public void InsertSignature(string inputPdfPath, string outputPdfPath, StrokeCollection strokes, double imgWidth, double imgHeight)
        {
            if (!File.Exists(inputPdfPath))
                throw new Exception("Không tìm thấy file PDF!");

            using var reader = new PdfReader(inputPdfPath);
            using var writer = new PdfWriter(outputPdfPath);
            using var pdfDoc = new PdfDocument(reader, writer);

            var page = pdfDoc.GetFirstPage();
            var pageSize = page.GetPageSize();
            var canvas = new PdfCanvas(page);

            // Tính scale dựa trên kích thước thực tế
            double scaleX = pageSize.GetWidth() / imgWidth;
            double scaleY = pageSize.GetHeight() / imgHeight;

            System.Diagnostics.Debug.WriteLine($"PDF size: {pageSize.GetWidth()}x{pageSize.GetHeight()}");
            System.Diagnostics.Debug.WriteLine($"Canvas size: {imgWidth}x{imgHeight}");
            System.Diagnostics.Debug.WriteLine($"Scale: X={scaleX:F4}, Y={scaleY:F4}");

            foreach (var stroke in strokes)
            {
                var points = stroke.StylusPoints;
                if (points.Count < 2)
                    continue;

                var color = stroke.DrawingAttributes.Color;
                canvas.SetStrokeColor(new iText.Kernel.Colors.DeviceRgb(color.R, color.G, color.B));
                canvas.SetLineWidth((float)stroke.DrawingAttributes.Width * 0.75f); // Điều chỉnh độ dày

                // Điểm đầu tiên
                var first = points[0];
                double startX = first.X * scaleX;
                double startY = pageSize.GetHeight() - (first.Y * scaleY); // Đảo Y từ dưới lên

                canvas.MoveTo((float)startX, (float)startY);

                // Vẽ các điểm tiếp theo
                for (int i = 1; i < points.Count; i++)
                {
                    double x = points[i].X * scaleX;
                    double y = pageSize.GetHeight() - (points[i].Y * scaleY); // Đảo Y từ dưới lên

                    canvas.LineTo((float)x, (float)y);
                }

                canvas.Stroke();
            }

            canvas.Release();
            System.Diagnostics.Debug.WriteLine("✅ Chữ ký đã được chèn vào đúng vị trí!");
        }
    }
}