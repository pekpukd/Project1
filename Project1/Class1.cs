using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace Petzold.SpanTheCells
{

    class BirdEventArg
    {
        public string Message { get; }
        public double TimeOfFligth { get; }
        public BirdEventArg(string message, double timeOfFligth)
        {
            Message = message;
            TimeOfFligth = timeOfFligth;
        }
    }

    class Bird
    {
        public static double g = 9.8;
        public static double step = 0.1;

        public double t, x, y, velocity, angle, k, m;
        List<double> X = new List<double>();
        List<double> Y = new List<double>();
        List<double> Velocity_X = new List<double>();
        List<double> Velocity_Y = new List<double>();

        public delegate void FallHandler(Bird sender, BirdEventArg e);
        public event FallHandler Notify;

        public Bird(double x = 0, double y = 0, double velocity = 0, double angle = 0, double k = 0, double m = 0, double t = 0)
        {
            this.x = x;
            this.y = y;
            this.velocity = velocity;
            this.angle = angle;
            this.k = k;
            this.t = t;
        }
        public void ReadInputData(string inputFile)
        {
            string[] lines = File.ReadAllLines(inputFile);
            velocity = double.Parse(lines[0]);
            angle = double.Parse(lines[1]);
            m = double.Parse(lines[2]);
            k = double.Parse(lines[3]);
        }
        public void WriteFlightData(string outputFile)
        {
            using (StreamWriter writer = new StreamWriter(outputFile))
            {
                angle = angle * Math.PI / 180.0;
                Velocity_X.Add((k * velocity * Math.Cos(angle)) / m);
                Velocity_Y.Add(velocity * Math.Sin(angle));
                X.Add(0);
                Y.Add(0);
                int totalSteps = (int)(100 / step);
                for (int i = 0; i <= totalSteps; i += 1)
                {
                    t += step;
                    Velocity_X.Add(Velocity_X[i] - step * (k * Velocity_X[i]) / m);
                    Velocity_Y.Add(Velocity_Y[i] - step * (g + k * Velocity_X[i] / m));
                    X.Add(X[i] + step * Velocity_X[i]);
                    Y.Add(Y[i] + step * Velocity_Y[i]);
                    writer.WriteLine($"t = {t:F2} c: x = {X[i]:F2} м, y = {Y[i]:F2} м");
                    if (Y[i] < 0)
                    {
                        double timeOfFlight = t - step;
                        double range = X[i - 1];
                        writer.WriteLine($"Время полета: {timeOfFlight:F2} сек");
                        writer.WriteLine($"Дальность броска: {range:F2} м");
                        Notify?.Invoke(this, new BirdEventArg($"Тело упало. Время падения {timeOfFlight:F2}", timeOfFlight));
                        break;
                    }
                }
            }
        }
    }

    public class SpanTheCells : Window
    {
        [STAThread]
        public static void Main()
        {
            Application app = new Application();
            app.Run(new SpanTheCells());
        }

        private List<TextBox> textBoxes = new List<TextBox>();
        private string[] astrLabel = { "_Скорость:",  "_Угол наклона:",
                "_Масса:",
                "_Коэфициент сопротивления воздуха:" };
        private TextBox txtOutput;

        public SpanTheCells()
        {
            Title = "Bird game";

            Grid grid = new Grid();
            grid.Margin = new Thickness(5);
            Content = grid;

            ColumnDefinition coldef = new ColumnDefinition();
            coldef.Width = new GridLength(200, GridUnitType.Auto);
            grid.ColumnDefinitions.Add(coldef);

            coldef = new ColumnDefinition();
            coldef.Width = GridLength.Auto;
            grid.ColumnDefinitions.Add(coldef);

            coldef = new ColumnDefinition();
            coldef.Width = new GridLength(100, GridUnitType.Auto);
            grid.ColumnDefinitions.Add(coldef);

            coldef = new ColumnDefinition();
            coldef.Width = new GridLength(100, GridUnitType.Star);
            grid.ColumnDefinitions.Add(coldef);


            for (int i = 0; i < 6; i++)
            {
                RowDefinition rowdef = new RowDefinition();
                rowdef.Height = GridLength.Auto;
                grid.RowDefinitions.Add(rowdef);
            }

            for (int i = 0; i < astrLabel.Length; i++)
            {
                Label lbl = new Label();
                lbl.Content = astrLabel[i];
                lbl.VerticalContentAlignment = VerticalAlignment.Center;
                grid.Children.Add(lbl);
                Grid.SetRow(lbl, i);
                Grid.SetColumn(lbl, 0);
                TextBox txtbox = new TextBox();
                txtbox.Margin = new Thickness(5);
                grid.Children.Add(txtbox);
                Grid.SetRow(txtbox, i);
                Grid.SetColumn(txtbox, 2);
                Grid.SetColumnSpan(txtbox, 3);
                textBoxes.Add(txtbox);
            }
            Button btn = new Button();
            btn.Content = "Запуск";
            btn.Margin = new Thickness(5);
            btn.IsDefault = true;
            btn.Click += buttonFly_Click;
            grid.Children.Add(btn);
            Grid.SetRow(btn, 5);
            Grid.SetColumn(btn, 2);
            grid.Children[1].Focus();

            txtOutput = new TextBox
            {
                Margin = new Thickness(5),
                IsReadOnly = true,
                VerticalScrollBarVisibility = ScrollBarVisibility.Visible,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Visible,
                TextWrapping = TextWrapping.Wrap,
            };

            grid.Children.Add(txtOutput);
            Grid.SetRow(txtOutput, 0);
            Grid.SetColumn(txtOutput, 3);
            Grid.SetRowSpan(txtOutput, 5);
        }

        private void buttonFly_Click(object sender, EventArgs a)
        {
            Bird bird = new Bird();
            string inputfile = "input.txt";
            string outputfile = "output.txt";
            using (StreamWriter writer = new StreamWriter(inputfile))
            {
                foreach (var txtBox in textBoxes)
                {
                    writer.WriteLine(txtBox.Text);
                }
            }
            bird.ReadInputData(inputfile);
            bird.WriteFlightData(outputfile);
            string outputContent = File.ReadAllText(outputfile);
            txtOutput.Text = outputContent;
        }
    }
}