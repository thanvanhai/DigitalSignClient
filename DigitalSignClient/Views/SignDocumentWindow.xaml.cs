using DigitalSignClient.Services;
using Microsoft.Web.WebView2.Core;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace DigitalSignClient.Views
{
    public partial class SignDocumentWindow : Window
    {
        private readonly string _pdfPath;
        private readonly Guid _documentId;
        private readonly PdfSignService _signService;
        private bool _isInkMode = false;

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
            InkArea.DefaultDrawingAttributes = new DrawingAttributes
            {
                Color = Colors.Blue,
                Width = 2,
                Height = 2,
                FitToCurve = true
            };
        }

        private async void SignDocumentWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadPdfAsync();
            ShowInstruction("👆 Nhấn 'Bật ký tay' để bắt đầu ký");
        }

        private async Task LoadPdfAsync()
        {
            await PdfViewer.EnsureCoreWebView2Async();
            string pdfJsRootFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "pdfjs");
            string pdfJsWebFolder = Path.Combine(pdfJsRootFolder, "web");

            PdfViewer.CoreWebView2.SetVirtualHostNameToFolderMapping(
                "appassets", pdfJsRootFolder, CoreWebView2HostResourceAccessKind.Allow);

            string tempPdfPath = Path.Combine(pdfJsWebFolder, "temp.pdf");
            File.Copy(_pdfPath, tempPdfPath, true);

            string viewerUrl = "https://appassets/web/viewer.html?file=temp.pdf";
            PdfViewer.Source = new Uri(viewerUrl);
        }

        private void ShowInstruction(string msg)
        {
            InstructionText.Text = msg;
            InstructionPanel.Visibility = Visibility.Visible;
        }

        private async void BtnToggleInk_Click(object sender, RoutedEventArgs e)
        {
            if (!_isInkMode)
            {
                await StartInkMode();
                BtnToggleInk.Content = "📄 Xem PDF";
                _isInkMode = true;
            }
            else
            {
                ExitInkMode();
                BtnToggleInk.Content = "✍️ Bật ký tay";
                _isInkMode = false;
            }
        }

        private async Task StartInkMode()
        {
            await CapturePdfSnapshot();

            PdfViewer.Visibility = Visibility.Collapsed;
            PdfSnapshot.Visibility = Visibility.Visible;
            InkArea.Visibility = Visibility.Visible;

            ShowInstruction("✍️ Ký tay trực tiếp lên tài liệu, sau đó nhấn 'Xác nhận ký'");
        }

        private void ExitInkMode()
        {
            PdfViewer.Visibility = Visibility.Visible;
            PdfSnapshot.Visibility = Visibility.Collapsed;
            InkArea.Visibility = Visibility.Collapsed;

            ShowInstruction("👆 Nhấn 'Bật ký tay' để bắt đầu ký");
        }

        private async Task CapturePdfSnapshot()
        {
            using var stream = new MemoryStream();
            await PdfViewer.CoreWebView2.CapturePreviewAsync(CoreWebView2CapturePreviewImageFormat.Png, stream);
            stream.Position = 0;

            var bmp = new BitmapImage();
            bmp.BeginInit();
            bmp.CacheOption = BitmapCacheOption.OnLoad;
            bmp.StreamSource = stream;
            bmp.EndInit();
            bmp.Freeze();

            PdfSnapshot.Source = bmp;
        }

        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            InkArea.Strokes.Clear();
            ShowInstruction("🗑️ Đã xóa chữ ký, vẽ lại nếu muốn");
        }

        private async void BtnConfirm_Click(object sender, RoutedEventArgs e)
        {
            await ConfirmSignatureAsync();
        }

        private async Task ConfirmSignatureAsync()
        {
            if (InkArea.Strokes.Count == 0)
            {
                MessageBox.Show("Bạn chưa ký tay!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                string signedOutput = Path.Combine(Path.GetTempPath(), $"signed_{DateTime.Now:yyyyMMddHHmmss}.pdf");

                // Lấy kích thước ảnh gốc từ BitmapImage, không phải ActualWidth/Height
                var bitmapSource = PdfSnapshot.Source as BitmapSource;
                double imageWidth = bitmapSource?.PixelWidth ?? PdfSnapshot.ActualWidth;
                double imageHeight = bitmapSource?.PixelHeight ?? PdfSnapshot.ActualHeight;

                _signService.InsertSignature(
                    _pdfPath,
                    signedOutput,
                    InkArea.Strokes,
                    imageWidth,
                    imageHeight
                );

                MessageBox.Show($"✅ Đã ký và lưu: {signedOutput}", "Thành công",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = signedOutput,
                    UseShellExecute = true
                });

                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi ký: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}