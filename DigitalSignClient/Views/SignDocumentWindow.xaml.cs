using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using DigitalSignClient.Services;
using Microsoft.Web.WebView2.Core;
using System.Windows.Controls;

namespace DigitalSignClient.Views
{
    public partial class SignDocumentWindow : Window
    {
        private readonly string _pdfPath;
        private readonly Guid _documentId;
        private double _clickX;
        private double _clickY;
        private readonly PdfSignService _signService;

        // ✅ Lưu kích thước thật của PDF
        private double _pdfActualWidth;
        private double _pdfActualHeight;

        public SignDocumentWindow(string pdfPath, Guid documentId)
        {
            InitializeComponent();
            _pdfPath = pdfPath;
            _documentId = documentId;
            _signService = new PdfSignService();

            ConfigureInkCanvas();
            Loaded += SignDocumentWindow_Loaded;
        }

        private void ConfigureInkCanvas()
        {
            InkArea.EditingMode = InkCanvasEditingMode.Ink;

            var drawingAttributes = new System.Windows.Ink.DrawingAttributes
            {
                Color = Colors.Blue,
                Width = 3,
                Height = 3,
                FitToCurve = true,
                IgnorePressure = false
            };

            InkArea.DefaultDrawingAttributes = drawingAttributes;
        }

        private async void SignDocumentWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadPdfAsync();
            ShowInstruction("👆 Bước 1: Click 'Bật ký tay' để bắt đầu");
        }

        private async Task LoadPdfAsync()
        {
            await PdfViewer.EnsureCoreWebView2Async();
            PdfViewer.DefaultBackgroundColor = System.Drawing.Color.Transparent;

            string pdfJsRootFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "pdfjs");
            string pdfJsWebFolder = Path.Combine(pdfJsRootFolder, "web");

            if (!Directory.Exists(pdfJsWebFolder))
            {
                MessageBox.Show($"Không tìm thấy thư mục: {pdfJsWebFolder}");
                return;
            }

            PdfViewer.CoreWebView2.SetVirtualHostNameToFolderMapping(
                "appassets", pdfJsRootFolder, CoreWebView2HostResourceAccessKind.Allow);

            string tempPdfPath = Path.Combine(pdfJsWebFolder, "temp.pdf");
            File.Copy(_pdfPath, tempPdfPath, true);

            string viewerUrl = "https://appassets/web/viewer.html?file=temp.pdf";
            PdfViewer.Source = new Uri(viewerUrl);

            // ✅ Lấy kích thước PDF thật
            await GetPdfDimensions();
        }

        private async Task GetPdfDimensions()
        {
            // Đợi PDF load xong
            await Task.Delay(2000);

            try
            {
                // ✅ Lấy kích thước PDF từ PDF.js qua JavaScript
                var script = @"
                    (function() {
                        if (PDFViewerApplication && PDFViewerApplication.pdfDocument) {
                            return PDFViewerApplication.pdfDocument.getPage(1).then(page => {
                                const viewport = page.getViewport({scale: 1.0});
                                return {width: viewport.width, height: viewport.height};
                            });
                        }
                        return null;
                    })();
                ";

                var result = await PdfViewer.CoreWebView2.ExecuteScriptAsync(script);
                System.Diagnostics.Debug.WriteLine($"PDF Dimensions: {result}");

                // Parse kết quả nếu cần
                // Hoặc dùng thư viện PDF để lấy kích thước
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting PDF dimensions: {ex.Message}");
            }
        }

        private void ShowInstruction(string message)
        {
            InstructionPanel.Visibility = Visibility.Visible;
            InstructionText.Text = message;
        }

        private void HideInstruction()
        {
            InstructionPanel.Visibility = Visibility.Collapsed;
        }

        private async void ToggleInk_Click(object sender, RoutedEventArgs e)
        {
            if (InkArea.Visibility == Visibility.Visible)
            {
                await SwitchToSelectPositionMode();
            }
            else
            {
                await SwitchToDrawingMode();
            }
        }

        private async Task SwitchToDrawingMode()
        {
            await CaptureWebView2();

            InkArea.Visibility = Visibility.Visible;
            ClickCanvas.Visibility = Visibility.Collapsed;
            PdfViewer.Visibility = Visibility.Collapsed;
            PdfSnapshot.Visibility = Visibility.Visible;

            BtnToggleInk.Content = "✓ Đang ký tay";
            BtnToggleInk.Background = new SolidColorBrush(Color.FromRgb(76, 175, 80));
            BtnToggleInk.Foreground = Brushes.White;
            BtnToggleInk.Visibility = Visibility.Collapsed;

            BtnDoneDrawing.Visibility = Visibility.Visible;

            ShowInstruction("✍️ Bước 2: Vẽ chữ ký của bạn, sau đó bấm 'Xong - Chọn vị trí'");
        }

        private async Task SwitchToSelectPositionMode()
        {
            InkArea.Visibility = Visibility.Collapsed;
            ClickCanvas.Visibility = Visibility.Visible;

            PdfViewer.Visibility = Visibility.Collapsed;
            PdfSnapshot.Visibility = Visibility.Visible;

            BtnToggleInk.Content = "🖊️ Sửa chữ ký";
            BtnToggleInk.Background = Brushes.White;
            BtnToggleInk.Foreground = Brushes.Black;
            BtnToggleInk.Visibility = Visibility.Visible;

            BtnDoneDrawing.Visibility = Visibility.Collapsed;

            if (InkArea.Strokes.Count > 0)
            {
                ShowInstruction("📍 Bước 3: Click vào vị trí trên PDF để đặt chữ ký");
            }
            else
            {
                ShowInstruction("⚠️ Bạn chưa vẽ chữ ký! Click 'Sửa chữ ký' để vẽ lại");
            }
        }

        private async void DoneDrawing_Click(object sender, RoutedEventArgs e)
        {
            if (InkArea.Strokes.Count == 0)
            {
                MessageBox.Show("Bạn chưa vẽ chữ ký! Vui lòng vẽ trước khi tiếp tục.",
                    "Chưa có chữ ký", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            await SwitchToSelectPositionMode();
        }

        private async Task CaptureWebView2()
        {
            try
            {
                using (var stream = new MemoryStream())
                {
                    await PdfViewer.CoreWebView2.CapturePreviewAsync(
                        CoreWebView2CapturePreviewImageFormat.Png, stream);

                    stream.Position = 0;
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.StreamSource = stream;
                    bitmap.EndInit();
                    bitmap.Freeze();

                    PdfSnapshot.Source = bitmap;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi chụp màn hình: {ex.Message}");
            }
        }

        private void ClickCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (InkArea.Visibility == Visibility.Visible)
                return;

            var pos = e.GetPosition(ClickCanvas);
            _clickX = pos.X;
            _clickY = pos.Y;

            System.Diagnostics.Debug.WriteLine($"Click position: X={_clickX}, Y={_clickY}");
            System.Diagnostics.Debug.WriteLine($"Canvas size: W={ClickCanvas.ActualWidth}, H={ClickCanvas.ActualHeight}");

            DrawMarker(pos.X, pos.Y);

            ShowInstruction("✅ Đã chọn vị trí! Bấm 'Xác nhận ký' để hoàn tất");
        }

        private void DrawMarker(double x, double y)
        {
            ClickCanvas.Children.Clear();

            var ellipse = new System.Windows.Shapes.Ellipse
            {
                Width = 30,
                Height = 30,
                Fill = Brushes.Red,
                Stroke = Brushes.White,
                StrokeThickness = 4,
                Opacity = 0.9
            };
            Canvas.SetLeft(ellipse, x - 15);
            Canvas.SetTop(ellipse, y - 15);
            ClickCanvas.Children.Add(ellipse);
        }

        private void ClearInk_Click(object sender, RoutedEventArgs e)
        {
            InkArea.Strokes.Clear();
            _clickX = 0;
            _clickY = 0;
            ClickCanvas.Children.Clear();

            PdfViewer.Visibility = Visibility.Visible;
            PdfSnapshot.Visibility = Visibility.Collapsed;

            ShowInstruction("🗑️ Đã xóa! Click 'Bật ký tay' để vẽ lại");
        }

        private async void Confirm_Click(object sender, RoutedEventArgs e)
        {
            if (InkArea.Strokes.Count == 0)
            {
                MessageBox.Show("Bạn chưa ký tay!", "Chưa có chữ ký", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (_clickX <= 0 || _clickY <= 0)
            {
                MessageBox.Show("Bạn chưa chọn vị trí đặt chữ ký!\n\nSau khi vẽ chữ ký, click vào vị trí trên PDF.",
                    "Chưa chọn vị trí", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                HideInstruction();

                // ✅ Lưu chữ ký thành PNG
                var signaturePath = Path.Combine(Path.GetTempPath(), "signature.png");
                SaveInkToPng(signaturePath);

                // ✅ Tạo file PDF đã ký
                var signedOutput = Path.Combine(Path.GetTempPath(), $"signed_{DateTime.Now:yyyyMMddHHmmss}.pdf");

                System.Diagnostics.Debug.WriteLine($"=== SIGNING PDF ===");
                System.Diagnostics.Debug.WriteLine($"Input PDF: {_pdfPath}");
                System.Diagnostics.Debug.WriteLine($"Output PDF: {signedOutput}");
                System.Diagnostics.Debug.WriteLine($"Signature: {signaturePath}");
                System.Diagnostics.Debug.WriteLine($"Position: X={_clickX}, Y={_clickY}");

                // ✅ CHÈN CHỮ KÝ VÀO PDF
                _signService.InsertSignature(_pdfPath, signedOutput, signaturePath, _clickX, _clickY);

                // Kiểm tra file đã được tạo
                if (!File.Exists(signedOutput))
                {
                    MessageBox.Show("Lỗi: File PDF đã ký không được tạo ra!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var fileInfo = new FileInfo(signedOutput);
                System.Diagnostics.Debug.WriteLine($"Output file size: {fileInfo.Length} bytes");

                // ✅ Upload lên server
                var api = new ApiService();
                var uploadSuccess = await api.UploadSignedDocumentAsync(_documentId, signedOutput);

                if (uploadSuccess)
                {
                    MessageBox.Show($"✓ Đã ký và lưu thành công!\n\nFile: {signedOutput}",
                        "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                    DialogResult = true;
                }
                else
                {
                    MessageBox.Show($"⚠ Chèn chữ ký thành công nhưng upload thất bại!\n\n" +
                        $"File đã lưu tại:\n{signedOutput}\n\n" +
                        $"Bạn có thể upload thủ công sau.",
                        "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                }

                // ✅ Mở file PDF để xem
                var viewResult = MessageBox.Show("Bạn có muốn mở file PDF đã ký để xem không?",
                    "Xem file", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (viewResult == MessageBoxResult.Yes)
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = signedOutput,
                        UseShellExecute = true
                    });
                }

                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"✗ Lỗi: {ex.Message}\n\nStack trace:\n{ex.StackTrace}",
                    "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveInkToPng(string file)
        {
            var bounds = InkArea.Strokes.GetBounds();

            if (bounds.Width <= 0 || bounds.Height <= 0)
            {
                throw new Exception("Chữ ký không hợp lệ!");
            }

            var bmp = new RenderTargetBitmap(
                (int)bounds.Width + 20,
                (int)bounds.Height + 20,
                96, 96,
                PixelFormats.Pbgra32);

            var dv = new DrawingVisual();
            using (var dc = dv.RenderOpen())
            {
                // Nền trắng
                dc.DrawRectangle(Brushes.White, null,
                    new Rect(0, 0, bounds.Width + 20, bounds.Height + 20));

                // Vẽ chữ ký
                dc.PushTransform(new TranslateTransform(-bounds.X + 10, -bounds.Y + 10));
                InkArea.Strokes.Draw(dc);
            }
            bmp.Render(dv);

            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bmp));
            using var fs = new FileStream(file, FileMode.Create);
            encoder.Save(fs);

            System.Diagnostics.Debug.WriteLine($"Signature saved: {file}");
            System.Diagnostics.Debug.WriteLine($"Signature size: {new FileInfo(file).Length} bytes");
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}