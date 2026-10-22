using Microsoft.Win32;
using System;
using System.IO;
using System.Text;
using System.Windows;

namespace Practice52
{
    public partial class MainWindow : Window
    {
        string filePath = "";

        public MainWindow()
        {
            InitializeComponent();
        }

        // Открытие файла
        private void Open_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Текстовые файлы (*.txt)|*.txt|Все файлы (*.*)|*.*";

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    filePath = dialog.FileName;
                    Editor.Text = File.ReadAllText(filePath, Encoding.UTF8);
                    FileName.Text = Path.GetFileName(filePath);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка открытия файла: " + ex.Message);
                }
            }
        }

        // Сохранение файла
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (filePath == "")
            {
                SaveFileDialog dialog = new SaveFileDialog();
                dialog.Filter = "Текстовые файлы (*.txt)|*.txt|Все файлы (*.*)|*.*";
                dialog.DefaultExt = ".txt";

                if (dialog.ShowDialog() != true)
                    return;

                filePath = dialog.FileName;
            }

            try
            {
                File.WriteAllText(filePath, Editor.Text, Encoding.UTF8);
                FileName.Text = Path.GetFileName(filePath);

                MessageBox.Show("Файл успешно сохранён!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка сохранения файла: " + ex.Message);
            }
        }
    }
}