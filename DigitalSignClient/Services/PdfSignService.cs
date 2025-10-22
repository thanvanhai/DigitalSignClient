using System;
using System.IO;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.IO.Image;
using iText.Kernel.Geom;

namespace DigitalSignClient.Services
{
    public class PdfSignService
    {
        public void InsertSignature(string inputPdfPath, string outputPdfPath, string signatureImagePath, double x, double y)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== INSERTING SIGNATURE ===");
                System.Diagnostics.Debug.WriteLine($"Input PDF: {inputPdfPath}");
                System.Diagnostics.Debug.WriteLine($"Output PDF: {outputPdfPath}");
                System.Diagnostics.Debug.WriteLine($"Signature Image: {signatureImagePath}");
                System.Diagnostics.Debug.WriteLine($"Position: X={x}, Y={y}");

                // Kiểm tra file tồn tại
                if (!File.Exists(inputPdfPath))
                {
                    throw new Exception($"File PDF không tồn tại: {inputPdfPath}");
                }

                if (!File.Exists(signatureImagePath))
                {
                    throw new Exception($"File chữ ký không tồn tại: {signatureImagePath}");
                }

                // Đọc PDF
                using (var reader = new PdfReader(inputPdfPath))
                using (var writer = new PdfWriter(outputPdfPath))
                using (var pdfDoc = new PdfDocument(reader, writer))
                {
                    // Lấy trang đầu tiên
                    var page = pdfDoc.GetFirstPage();
                    var pageSize = page.GetPageSize();

                    System.Diagnostics.Debug.WriteLine($"Page size: {pageSize.GetWidth()} x {pageSize.GetHeight()}");

                    // Tạo document
                    var document = new Document(pdfDoc);

                    // Load ảnh chữ ký
                    var imageData = ImageDataFactory.Create(signatureImagePath);
                    var signatureImage = new Image(imageData);

                    // Kích thước chữ ký (có thể điều chỉnh)
                    float signatureWidth = 150f;
                    float signatureHeight = 75f;
                    signatureImage.ScaleToFit(signatureWidth, signatureHeight);

                    // ✅ QUAN TRỌNG: Tọa độ PDF
                    // - PDF: gốc tọa độ ở DƯỚI-TRÁI, Y tăng đi lên
                    // - WPF: gốc tọa độ ở TRÊN-TRÁI, Y tăng đi xuống
                    // => Phải đảo ngược trục Y

                    float pdfX = (float)x;
                    float pdfY = (float)(pageSize.GetHeight() - y - signatureHeight);

                    System.Diagnostics.Debug.WriteLine($"PDF coordinates: X={pdfX}, Y={pdfY}");

                    // Đặt vị trí chữ ký
                    signatureImage.SetFixedPosition(1, pdfX, pdfY);

                    // Thêm vào document
                    document.Add(signatureImage);
                    document.Close();

                    System.Diagnostics.Debug.WriteLine("✅ Signature inserted successfully!");
                }

                // Kiểm tra file output
                if (File.Exists(outputPdfPath))
                {
                    var fileInfo = new FileInfo(outputPdfPath);
                    System.Diagnostics.Debug.WriteLine($"Output file created: {fileInfo.Length} bytes");
                }
                else
                {
                    throw new Exception("Output PDF was not created!");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ ERROR: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                throw new Exception($"Lỗi khi chèn chữ ký: {ex.Message}", ex);
            }
        }
    }
}