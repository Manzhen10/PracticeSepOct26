
using System;
using System.Globalization;
using System.Windows;

namespace Practice55
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        // Сложение
        private void Plus_Click(object sender, RoutedEventArgs e)
        {
            Calculate("+");
        }

        // Вычитание
        private void Minus_Click(object sender, RoutedEventArgs e)
        {
            Calculate("-");
        }

        // Умножение
        private void Umnozh_Click(object sender, RoutedEventArgs e)
        {
            Calculate("*");
        }

        // Деление
        private void Delen_Click(object sender, RoutedEventArgs e)
        {
            Calculate("/");
        }

        // Вычисление
        private void Calculate(string action)
        {
            string text1 = Num1.Text.Trim().Replace(',', '.');
            string text2 = Num2.Text.Trim().Replace(',', '.');

            if (!double.TryParse(text1, NumberStyles.Float,
                    CultureInfo.InvariantCulture, out double a) ||
                !double.TryParse(text2, NumberStyles.Float,
                    CultureInfo.InvariantCulture, out double b))
            {
                MessageBox.Show("Введите два правильных числа!");
                return;
            }

            double answer;

            try
            {
                switch (action)
                {
                    case "+":
                        answer = new Plus().Calculate(a, b);
                        break;

                    case "-":
                        answer = new Minus().Calculate(a, b);
                        break;

                    case "*":
                        answer = new Umnozh().Calculate(a, b);
                        break;

                    case "/":
                        answer = new Delen().Calculate(a, b);
                        break;

                    default:
                        return;
                }
            }
            catch (DivideByZeroException)
            {
                MessageBox.Show("На ноль делить нельзя!");
                return;
            }

            ResultText.Text = $"Результат: {answer}";
        }
    }
}
