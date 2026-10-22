
using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Practice54
{
    public partial class MainWindow : Window
    {
        double width;
        double height;

        public MainWindow()
        {
            InitializeComponent();
        }

        // Открытие изображения
        private void Open_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Изображения|*.png;*.jpg;*.jpeg;*.bmp;*.gif";

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    BitmapImage image = new BitmapImage();

                    image.BeginInit();
                    image.UriSource = new Uri(dialog.FileName);
                    image.CacheOption = BitmapCacheOption.OnLoad;
                    image.EndInit();

                    ImageViewer.Source = image;

                    double scale = Math.Min(
                        1.0,
                        Math.Min(800.0 / image.PixelWidth,
                                 400.0 / image.PixelHeight));

                    width = image.PixelWidth * scale;
                    height = image.PixelHeight * scale;

                    ZoomSlider.Value = 1;
                    UpdateZoom();

                    FileName.Text = Path.GetFileName(dialog.FileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка открытия изображения: " + ex.Message);
                }
            }
        }

        // Изменение масштаба
        private void ZoomSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            UpdateZoom();
        }

        // Обновление размера изображения
        private void UpdateZoom()
        {
            if (ImageViewer == null || ImageViewer.Source == null)
                return;

            ImageViewer.Width = width * ZoomSlider.Value;
            ImageViewer.Height = height * ZoomSlider.Value;

            ZoomText.Text = $"{ZoomSlider.Value * 100:F0}%";
        }
    }
}
