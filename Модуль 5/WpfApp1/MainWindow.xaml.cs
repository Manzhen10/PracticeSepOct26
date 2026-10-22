using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace WpfApp1
{
    public partial class MainWindow : Window
    {
        Point start;
        Shape shape;
        bool draw = false;

        public MainWindow()
        {
            InitializeComponent();
        }

        // Начало рисования
        private void Down(object sender, MouseButtonEventArgs e)
        {
            start = e.GetPosition(board);
            draw = true;
            board.CaptureMouse();

            switch (kind.SelectedIndex)
            {
                case 0:
                    shape = new Line
                    {
                        X1 = start.X,
                        Y1 = start.Y,
                        X2 = start.X,
                        Y2 = start.Y,
                        Stroke = Brushes.Blue,
                        StrokeThickness = 3
                    };
                    break;

                case 1:
                    shape = new Ellipse
                    {
                        Stroke = Brushes.Blue,
                        StrokeThickness = 3,
                        Fill = Brushes.LightBlue
                    };
                    break;

                case 2:
                    shape = new Rectangle
                    {
                        Stroke = Brushes.Blue,
                        StrokeThickness = 3,
                        Fill = Brushes.LightBlue
                    };
                    break;
            }

            board.Children.Add(shape);
        }

        // Движение мыши
        private void Move(object sender, MouseEventArgs e)
        {
            if (!draw || shape == null)
                return;

            Point end = e.GetPosition(board);

            if (shape is Line)
            {
                Line line = (Line)shape;
                line.X2 = end.X;
                line.Y2 = end.Y;
            }
            else
            {
                double size = Math.Max(
                    Math.Abs(end.X - start.X),
                    Math.Abs(end.Y - start.Y));

                Canvas.SetLeft(shape, Math.Min(start.X, end.X));
                Canvas.SetTop(shape, Math.Min(start.Y, end.Y));

                shape.Width = size;
                shape.Height = size;
            }
        }

        // Завершение рисования
        private void Up(object sender, MouseButtonEventArgs e)
        {
            draw = false;
            board.ReleaseMouseCapture();
            shape = null;
        }

        // Очистка холста
        private void Clear(object sender, RoutedEventArgs e)
        {
            board.Children.Clear();
        }
    }
}