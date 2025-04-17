using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows;

namespace Project1
{
    public partial class SpanTheCells : Window
    {
        void OnOpen(object sender, RoutedEventArgs args)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.CheckFileExists = true;

            if ((bool)dlg.ShowDialog(this))
            {
                Bird bird = new Bird();
                string inputFile = dlg.FileName;
                string[] inputData = bird.ReadInputData(inputFile);
                for (int i = 0; i < inputData.Length; i++)
                {
                    textBoxes[i].Text = inputData[i];
                }
            }
        }
        void UnimplementedOnClick(object sender, RoutedEventArgs args)

        {
            MenuItem item = sender as MenuItem;
            if (item.Header == "_Open")
            {
                OnOpen(sender, args);
                buttonFly_Click(sender, args);
            }
        }
        void ExitOnClick(object sender, RoutedEventArgs args)   //закрывает окно
        {
            Close();
        }
        void TaskbarOnClick(object sender, RoutedEventArgs args)
        {
            MenuItem item = sender as MenuItem;
            ShowInTaskbar = item.IsChecked;
        }
        void SizeOnCheck(object sender, RoutedEventArgs args)
        {
            MenuItem item = sender as MenuItem;
            SizeToContent = item.IsChecked ? SizeToContent.WidthAndHeight :
                SizeToContent.Manual;
        }
        void ResizeOnClick(object sender, RoutedEventArgs args)
        {
            MenuItem item = sender as MenuItem;
            ResizeMode = item.IsChecked ? ResizeMode.CanResize :
                ResizeMode.NoResize;
        }
        void TopmostOnCheck(object sender, RoutedEventArgs args)
        {
            MenuItem item = sender as MenuItem;
            Topmost = item.IsChecked;
        }

        private void buttonFly_Click(object sender, EventArgs a)
        {
            Bird bird = new Bird();
            bird.Notify += DisplayMessage;

            string outputfile = "output.txt";
            bird.ReadFromTextBox(textBoxes);
            bird.WriteFlightData(outputfile);
            string outputContent = File.ReadAllText(outputfile);
            txtOutput.Text = outputContent;

            List<double> X = bird.GetX();
            List<double> Y = bird.GetY();

            DrawLines(canv, X, Y);
        }
        void DisplayMessage(Bird sender, BirdEventArg e, StreamWriter writer)
        {
            writer.WriteLine(e.Message);
        }
        async void DrawLines(Canvas canv, List<double> X, List<double> Y)
        {
            canv.Children.Clear();

            double maxX = X.Max();
            double maxY = Y.Max();

            double k_x = (canv.ActualWidth) / (maxX);
            double k_y = (canv.ActualHeight) / (maxY);
            double k = Math.Min(k_x, k_y);

            for (int i = 0; i < X.Count - 1 & Y[i + 1] >= 0; i++)
            {
                Line line = new Line
                {
                    X1 = X[i] * k,
                    Y1 = canv.ActualHeight - Y[i] * k,
                    X2 = X[i + 1] * k,
                    Y2 = canv.ActualHeight - Y[i + 1] * k,
                    Stroke = Brushes.Green,
                    StrokeThickness = 4
                };
                canv.Children.Add(line);
                await Task.Delay(1);
            }
        }
    }
}
