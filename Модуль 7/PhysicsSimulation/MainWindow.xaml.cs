using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace PhysicsSimulation
{
    public partial class MainWindow : Window
    {
        // Физические параметры
        private double R = 0.1;
        private double M = 0.1;
        private double g = 9.81;
        private double e = 0.8;
        private double mu = 0.1;
        private double surfaceAngleDeg = 0;

        // Состояние симуляции
        private double x, y;
        private double vx, vy;
        private bool isSimulating;
        private bool isPaused;
        private bool isBallPlaced;
        private bool hasBounced;

        // Состояние прицеливания
        private bool isAiming;
        private Point aimCurrentMouse;
        private const double VelocityScale = 0.1;

        private double currentFlightMaxHeight;
        private double lastBounceHeight;
        private readonly List<Point> trajectory = new List<Point>();

        // Статистика
        private double simTime;
        private double totalDistance;
        private double minSpeed = double.MaxValue;
        private double maxSpeed;
        private double startHeight;

        private const double scale = 100.0;
        private const double floorOffsetMeters = 1.0;
        private const double dt = 0.0001;

        private readonly DispatcherTimer simTimer;
        private readonly Stopwatch realTimeTracker;
        private double accumulatedRealTime;

        public MainWindow()
        {
            InitializeComponent();

            realTimeTracker = new Stopwatch();
            simTimer = new DispatcherTimer(DispatcherPriority.Render)
            {
                Interval = TimeSpan.FromMilliseconds(16)
            };
            simTimer.Tick += SimTimer_Tick;

            canvas.DrawScene = DrawScene;
            UpdateStats();
        }

        private void ChkShowTrajectory_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (canvas != null)
                canvas.InvalidateVisual();
        }

        private void BtnExport_Click(object sender, RoutedEventArgs e)
        {
            if (isSimulating && !isPaused)
            {
                MessageBox.Show(this,
                    "Экспорт доступен только во время паузы или после остановки симуляции.",
                    "Внимание", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (!isBallPlaced)
            {
                MessageBox.Show(this,
                    "Нет данных для экспорта. Сначала разместите шар и запустите симуляцию.",
                    "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            SaveFileDialog dialog = new SaveFileDialog
            {
                Filter = "Текстовые файлы (*.txt)|*.txt",
                Title = "Сохранить результаты симуляции",
                FileName = "Результаты_симуляции.txt",
                DefaultExt = ".txt",
                AddExtension = true
            };

            if (dialog.ShowDialog(this) != true)
                return;

            try
            {
                string report = "=== РЕЗУЛЬТАТЫ ФИЗИЧЕСКОЙ СИМУЛЯЦИИ ===\n" +
                                string.Format(CultureInfo.CurrentCulture, "Дата экспорта: {0}\n\n", DateTime.Now) +
                                "--- Вводные параметры ---\n" +
                                string.Format(CultureInfo.CurrentCulture, "Радиус (R): {0} м\n", R) +
                                string.Format(CultureInfo.CurrentCulture, "Масса (M): {0} кг\n", M) +
                                string.Format(CultureInfo.CurrentCulture, "Ускорение свободного падения (g): {0} м/с²\n", g) +
                                string.Format(CultureInfo.CurrentCulture, "Коэфф. восстановления (e): {0}\n", e) +
                                string.Format(CultureInfo.CurrentCulture, "Коэфф. трения (mu): {0}\n", mu) +
                                string.Format(CultureInfo.CurrentCulture, "Угол наклона поверхности: {0}°\n\n", surfaceAngleDeg) +
                                "--- Статистика ---\n" + lblStats.Text;

                File.WriteAllText(dialog.FileName, report);
                MessageBox.Show(this, "Данные успешно сохранены!", "Экспорт завершен",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Ошибка при сохранении файла: " + ex.Message,
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnScreenshot_Click(object sender, RoutedEventArgs e)
        {
            int width = (int)Math.Ceiling(ActualWidth);
            int height = (int)Math.Ceiling(ActualHeight);
            if (width <= 0 || height <= 0)
                return;

            SaveFileDialog dialog = new SaveFileDialog
            {
                Filter = "Изображение PNG (*.png)|*.png|Изображение JPEG (*.jpg)|*.jpg",
                Title = "Сохранить скриншот",
                FileName = string.Format("Скриншот_{0:yyyyMMdd_HHmmss}.png", DateTime.Now),
                DefaultExt = ".png",
                AddExtension = true
            };

            if (dialog.ShowDialog(this) != true)
                return;

            try
            {
                UpdateLayout();
                RenderTargetBitmap bitmap = new RenderTargetBitmap(
                    width, height, 96, 96, PixelFormats.Pbgra32);
                bitmap.Render(this);

                BitmapEncoder encoder;
                string extension = Path.GetExtension(dialog.FileName).ToLowerInvariant();
                if (extension == ".jpg" || extension == ".jpeg")
                {
                    JpegBitmapEncoder jpegEncoder = new JpegBitmapEncoder { QualityLevel = 95 };
                    jpegEncoder.Frames.Add(BitmapFrame.Create(bitmap));
                    encoder = jpegEncoder;
                }
                else
                {
                    PngBitmapEncoder pngEncoder = new PngBitmapEncoder();
                    pngEncoder.Frames.Add(BitmapFrame.Create(bitmap));
                    encoder = pngEncoder;
                }

                using (FileStream stream = new FileStream(dialog.FileName, FileMode.Create, FileAccess.Write))
                    encoder.Save(stream);

                MessageBox.Show(this, "Скриншот успешно сохранён!", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Ошибка при создании скриншота: " + ex.Message,
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (isSimulating && !isPaused)
                return;

            ReadParameters();
            Point mouse = e.GetPosition(canvas);

            if (e.ChangedButton == MouseButton.Right && isBallPlaced)
            {
                isAiming = true;
                aimCurrentMouse = mouse;
                canvas.CaptureMouse();
                canvas.InvalidateVisual();
                e.Handled = true;
                return;
            }

            if (e.ChangedButton == MouseButton.Left)
            {
                x = mouse.X / scale;
                y = (canvas.ActualHeight - mouse.Y) / scale - floorOffsetMeters;

                double alpha = surfaceAngleDeg * Math.PI / 180.0;
                double t = x * Math.Cos(alpha) + y * Math.Sin(alpha);
                double dist = y * Math.Cos(alpha) - x * Math.Sin(alpha);

                if (t >= 0 && dist < R)
                {
                    x += (R - dist) * (-Math.Sin(alpha));
                    y += (R - dist) * Math.Cos(alpha);
                }

                isBallPlaced = true;
                hasBounced = false;
                trajectory.Clear();

                currentFlightMaxHeight = y * Math.Cos(alpha) - x * Math.Sin(alpha);
                lastBounceHeight = 0;
                vx = 0;
                vy = 0;
                startHeight = y * Math.Cos(alpha) - x * Math.Sin(alpha);
                UpdateStats();
                canvas.InvalidateVisual();
                e.Handled = true;
            }
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (isAiming)
            {
                aimCurrentMouse = e.GetPosition(canvas);
                canvas.InvalidateVisual();
            }
        }

        private void Canvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (isAiming && e.ChangedButton == MouseButton.Right)
            {
                isAiming = false;
                canvas.ReleaseMouseCapture();

                Point mouse = e.GetPosition(canvas);
                double baseY = canvas.ActualHeight - floorOffsetMeters * scale;
                double ballScreenX = x * scale;
                double ballScreenY = baseY - y * scale;

                double dxScreen = ballScreenX - mouse.X;
                double dyScreen = ballScreenY - mouse.Y;
                double physDx = dxScreen;
                double physDy = -dyScreen;

                double distance = Math.Sqrt(physDx * physDx + physDy * physDy);
                double v0 = distance * VelocityScale;
                double angleRad = Math.Atan2(physDy, physDx);
                double angleDeg = angleRad * 180.0 / Math.PI;
                if (angleDeg < 0)
                    angleDeg += 360.0;

                txtV0.Text = v0.ToString("0.##", CultureInfo.CurrentCulture);
                txtAngleLaunch.Text = angleDeg.ToString("0.##", CultureInfo.CurrentCulture);
                canvas.InvalidateVisual();
                e.Handled = true;
            }
        }

        private void ReadParameters()
        {
            R = ReadDouble(txtRadius.Text, R);
            M = ReadDouble(txtMass.Text, M);
            e = ReadDouble(txtRestitution.Text, e);
            mu = ReadDouble(txtFriction.Text, mu);
            g = ReadDouble(txtGravity.Text, g);
            surfaceAngleDeg = ReadDouble(txtAngleSurface.Text, surfaceAngleDeg);
        }

        private static double ReadDouble(string text, double fallback)
        {
            double value;
            if (double.TryParse(text, NumberStyles.Float, CultureInfo.CurrentCulture, out value))
                return value;
            if (double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out value))
                return value;

            string normalized = (text ?? string.Empty).Trim().Replace(',', '.');
            if (double.TryParse(normalized, NumberStyles.Float, CultureInfo.InvariantCulture, out value))
                return value;

            return fallback;
        }

        private void BtnStart_Click(object sender, RoutedEventArgs e)
        {
            if (!isBallPlaced)
            {
                MessageBox.Show(this, "Сначала установите шар, щёлкнув по рабочей области левой кнопкой мыши!",
                    "Внимание", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            ReadParameters();
            double v0 = ReadDouble(txtV0.Text, 0.0);
            double angleLaunchDeg = ReadDouble(txtAngleLaunch.Text, 0.0);
            double angleLaunchRad = angleLaunchDeg * Math.PI / 180.0;
            vx = v0 * Math.Cos(angleLaunchRad);
            vy = v0 * Math.Sin(angleLaunchRad);

            simTime = 0;
            totalDistance = 0;
            maxSpeed = v0;
            minSpeed = v0 > 0 ? v0 : double.MaxValue;
            lastBounceHeight = 0;
            hasBounced = false;
            trajectory.Clear();
            trajectory.Add(new Point(x, y));

            double alpha = surfaceAngleDeg * Math.PI / 180.0;
            startHeight = y * Math.Cos(alpha) - x * Math.Sin(alpha);
            currentFlightMaxHeight = y * Math.Cos(alpha) - x * Math.Sin(alpha);

            accumulatedRealTime = 0;
            isSimulating = true;
            isPaused = false;
            btnPause.Content = "ПАУЗА";

            realTimeTracker.Restart();
            simTimer.Start();
            UpdateStats();
            canvas.InvalidateVisual();
        }

        private void BtnPause_Click(object sender, RoutedEventArgs e)
        {
            if (!isSimulating)
                return;

            if (isPaused)
            {
                isPaused = false;
                btnPause.Content = "ПАУЗА";
                realTimeTracker.Start();
                simTimer.Start();
            }
            else
            {
                isPaused = true;
                btnPause.Content = "ПРОДОЛЖИТЬ";
                realTimeTracker.Stop();
                simTimer.Stop();
                UpdateStats();
            }
        }

        private void BtnReset_Click(object sender, RoutedEventArgs e)
        {
            isSimulating = false;
            isPaused = false;
            isBallPlaced = false;
            isAiming = false;
            btnPause.Content = "ПАУЗА";
            simTimer.Stop();
            realTimeTracker.Stop();

            vx = 0;
            vy = 0;
            simTime = 0;
            totalDistance = 0;
            maxSpeed = 0;
            minSpeed = double.MaxValue;
            startHeight = 0;
            lastBounceHeight = 0;
            hasBounced = false;
            trajectory.Clear();
            UpdateStats();
            canvas.InvalidateVisual();
        }

        private void SimTimer_Tick(object sender, EventArgs e)
        {
            if (!isSimulating || isPaused)
                return;

            double realDelta = realTimeTracker.Elapsed.TotalSeconds;
            realTimeTracker.Restart();
            if (realDelta > 0.1)
                realDelta = 0.1;

            accumulatedRealTime += realDelta;
            while (simTime < accumulatedRealTime && isSimulating && !isPaused)
                UpdatePhysics(dt);

            UpdateStats();
            canvas.InvalidateVisual();
        }

        private void UpdatePhysics(double timeStep)
        {
            vy -= g * timeStep;

            double nextX = x + vx * timeStep;
            double nextY = y + vy * timeStep;
            double alpha = surfaceAngleDeg * Math.PI / 180.0;
            double distanceToSurface = nextY * Math.Cos(alpha) - nextX * Math.Sin(alpha);
            double projection = nextX * Math.Cos(alpha) + nextY * Math.Sin(alpha);
            bool isOverPlatform = projection >= 0;

            if (isOverPlatform && distanceToSurface > currentFlightMaxHeight)
                currentFlightMaxHeight = distanceToSurface;

            if (distanceToSurface <= R && isOverPlatform)
            {
                double nx = -Math.Sin(alpha);
                double ny = Math.Cos(alpha);
                double tx = Math.Cos(alpha);
                double ty = Math.Sin(alpha);
                double vn = vx * nx + vy * ny;
                double vt = vx * tx + vy * ty;

                if (vn < 0)
                {
                    if (hasBounced)
                        lastBounceHeight = currentFlightMaxHeight;
                    else
                        hasBounced = true;

                    currentFlightMaxHeight = R;
                    double vnNew = -e * vn;
                    double dVtMax = mu * (vnNew - vn);
                    double vtNew = Math.Abs(vt) <= dVtMax ? 0 : vt - Math.Sign(vt) * dVtMax;
                    vx = vnNew * nx + vtNew * tx;
                    vy = vnNew * ny + vtNew * ty;

                    double penetration = R - distanceToSurface;
                    nextX += penetration * nx;
                    nextY += penetration * ny;
                }
            }

            double rightBound = canvas.ActualWidth / scale;
            double topBound = (canvas.ActualHeight / scale) - floorOffsetMeters;
            double bottomBound = -floorOffsetMeters;

            if (nextX + R > rightBound)
            {
                nextX = rightBound - R;
                vx = -vx * e;
            }
            else if (nextX - R < 0)
            {
                nextX = R;
                vx = -vx * e;
            }

            if (nextY + R > topBound)
            {
                nextY = topBound - R;
                vy = -vy * e;
            }
            else if (nextY - R < bottomBound)
            {
                nextY = bottomBound + R;
                vy = -vy * e;
            }

            x = nextX;
            y = nextY;

            if (trajectory.Count == 0 ||
                Math.Abs(trajectory[trajectory.Count - 1].X - x) > 0.005 ||
                Math.Abs(trajectory[trajectory.Count - 1].Y - y) > 0.005)
            {
                trajectory.Add(new Point(x, y));
            }

            double speed = Math.Sqrt(vx * vx + vy * vy);
            if (speed > maxSpeed)
                maxSpeed = speed;
            if (speed > 0.01 && speed < minSpeed)
                minSpeed = speed;

            totalDistance += speed * timeStep;
            simTime += timeStep;

            if (speed < 0.007)
            {
                if ((isOverPlatform && distanceToSurface <= R + 0.005) || (y - R <= bottomBound + 0.005))
                {
                    isSimulating = false;
                    simTimer.Stop();
                    realTimeTracker.Stop();
                    vx = 0;
                    vy = 0;
                }
            }
        }

        private void UpdateStats()
        {
            if (lblStats == null)
                return;

            double currentSpeed = Math.Sqrt(vx * vx + vy * vy);
            double currentMin = minSpeed == double.MaxValue ? 0 : minSpeed;
            double vAvg = totalDistance / (simTime > 0 ? simTime : 1);
            double alpha = surfaceAngleDeg * Math.PI / 180.0;
            double heightFromSurface = y * Math.Cos(alpha) - x * Math.Sin(alpha);

            lblStats.Text = string.Format(CultureInfo.CurrentCulture,
                "Нач. высота: {0:F2} м\n" +
                "Тек. скорость: {1:F3} м/с\n" +
                "Vср: {2:F3} м/с\n" +
                "Vmax: {3:F3} м/с\n" +
                "Vmin: {4:F3} м/с\n" +
                "Время t: {5:F2} с\n" +
                "Тек. высота: {6:F2} м\n" +
                "Высота отскока (по центру): {7:F3} м",
                startHeight, currentSpeed, vAvg, maxSpeed, currentMin,
                simTime, heightFromSurface, lastBounceHeight);
        }

        private void DrawScene(DrawingContext drawingContext, Size size)
        {
            double width = size.Width;
            double height = size.Height;
            drawingContext.DrawRectangle(new SolidColorBrush(Color.FromRgb(28, 28, 28)), null,
                new Rect(0, 0, width, height));
            if (width <= 0 || height <= 0)
                return;

            Pen gridPen = new Pen(new SolidColorBrush(Color.FromArgb(40, 255, 255, 255)), 1.0);
            Brush meterBrush = new SolidColorBrush(Color.FromArgb(120, 255, 255, 255));
            double baseY = height - floorOffsetMeters * scale;

            for (double px = 0; px < width; px += scale)
            {
                drawingContext.DrawLine(gridPen, new Point(px, 0), new Point(px, height));
                if (px > 0)
                    DrawText(drawingContext, string.Format(CultureInfo.CurrentCulture, "{0} м", (int)(px / scale)),
                        meterBrush, new Point(px + 4, baseY + 4), 11);
            }

            int maxMetersY = (int)(height / scale) + 1;
            for (int meter = -1; meter <= maxMetersY; meter++)
            {
                double yScreen = baseY - meter * scale;
                if (yScreen >= 0 && yScreen <= height)
                {
                    drawingContext.DrawLine(gridPen, new Point(0, yScreen), new Point(width, yScreen));
                    DrawText(drawingContext, string.Format(CultureInfo.CurrentCulture, "{0} м", meter),
                        meterBrush, new Point(4, yScreen - 16), 11);
                }
            }

            double alpha = surfaceAngleDeg * Math.PI / 180.0;
            double drawLengthPixels = width * 2.0;
            Point surfaceStart = new Point(0, baseY);
            Point surfaceEnd = new Point(drawLengthPixels * Math.Cos(alpha),
                baseY - drawLengthPixels * Math.Sin(alpha));
            drawingContext.DrawLine(new Pen(new SolidColorBrush(Color.FromArgb(200, 200, 200, 200)), 5),
                surfaceStart, surfaceEnd);

            if (chkShowTrajectory != null && chkShowTrajectory.IsChecked == true && trajectory.Count > 1)
            {
                StreamGeometry geometry = new StreamGeometry();
                using (StreamGeometryContext context = geometry.Open())
                {
                    Point first = ToScreen(trajectory[0], baseY);
                    context.BeginFigure(first, false, false);
                    for (int i = 1; i < trajectory.Count; i++)
                        context.LineTo(ToScreen(trajectory[i], baseY), true, false);
                }
                geometry.Freeze();
                drawingContext.DrawGeometry(null,
                    new Pen(new SolidColorBrush(Color.FromArgb(230, 0, 229, 255)), 2.5), geometry);
            }

            if (!isBallPlaced)
                return;

            double screenR = R * scale;
            double screenX = x * scale;
            double screenY = baseY - y * scale;
            Rect ballRect = new Rect(screenX - screenR, screenY - screenR, screenR * 2, screenR * 2);
            RadialGradientBrush ballBrush = new RadialGradientBrush
            {
                Center = new Point(0.32, 0.32),
                GradientOrigin = new Point(0.32, 0.32),
                RadiusX = 0.8,
                RadiusY = 0.8
            };
            ballBrush.GradientStops.Add(new GradientStop(Color.FromRgb(255, 82, 82), 0.0));
            ballBrush.GradientStops.Add(new GradientStop(Color.FromRgb(139, 0, 0), 1.0));
            drawingContext.DrawEllipse(ballBrush, null, new Point(screenX, screenY), screenR, screenR);

            if (isAiming)
            {
                double dx = screenX - aimCurrentMouse.X;
                double dy = screenY - aimCurrentMouse.Y;
                DrawArrow(drawingContext, new Point(screenX, screenY),
                    new Point(screenX + dx, screenY + dy), Color.FromRgb(255, 152, 0), 3.0, 10.0);
            }
            else if (!isSimulating && !string.IsNullOrWhiteSpace(txtV0.Text))
            {
                double v0 = ReadDouble(txtV0.Text, 0.0);
                double angleLaunchDeg = ReadDouble(txtAngleLaunch.Text, 0.0);
                if (v0 > 0)
                {
                    double angle = angleLaunchDeg * Math.PI / 180.0;
                    double dirX = Math.Cos(angle);
                    double dirY = Math.Sin(angle);
                    double visualLength = v0 / VelocityScale;
                    DrawArrow(drawingContext, new Point(screenX, screenY),
                        new Point(screenX + dirX * visualLength, screenY - dirY * visualLength),
                        Color.FromRgb(220, 220, 220), 2.0, 8.0);
                }
            }
        }

        private static Point ToScreen(Point point, double baseY)
        {
            return new Point(point.X * scale, baseY - point.Y * scale);
        }

        private static void DrawText(DrawingContext dc, string text, Brush brush, Point origin, double fontSize)
        {
            FormattedText formatted = new FormattedText(text, CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight, new Typeface("Segoe UI"), fontSize, brush);
            dc.DrawText(formatted, origin);
        }

        private static void DrawArrow(DrawingContext dc, Point start, Point end,
            Color color, double thickness, double arrowLength)
        {
            Vector direction = end - start;
            if (direction.Length < 0.5)
                return;

            direction.Normalize();
            Vector perpendicular = new Vector(-direction.Y, direction.X);
            Point arrowBase = end - direction * arrowLength;
            Point left = arrowBase + perpendicular * (arrowLength * 0.45);
            Point right = arrowBase - perpendicular * (arrowLength * 0.45);

            Pen pen = new Pen(new SolidColorBrush(color), thickness)
            {
                StartLineCap = PenLineCap.Round,
                EndLineCap = PenLineCap.Round,
                LineJoin = PenLineJoin.Round
            };
            dc.DrawLine(pen, start, end - direction * (arrowLength * 0.6));

            StreamGeometry head = new StreamGeometry();
            using (StreamGeometryContext context = head.Open())
            {
                context.BeginFigure(end, true, true);
                context.LineTo(left, true, false);
                context.LineTo(right, true, false);
            }
            head.Freeze();
            dc.DrawGeometry(new SolidColorBrush(color), null, head);
        }

        protected override void OnClosed(EventArgs e)
        {
            simTimer.Stop();
            realTimeTracker.Stop();
            simTimer.Tick -= SimTimer_Tick;
            base.OnClosed(e);
        }
    }

    public sealed class SimulationSurface : FrameworkElement
    {
        public Action<DrawingContext, Size> DrawScene { get; set; }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            if (DrawScene != null)
                DrawScene(drawingContext, RenderSize);
            else
                drawingContext.DrawRectangle(new SolidColorBrush(Color.FromRgb(28, 28, 28)), null,
                    new Rect(RenderSize));
        }
    }
}
