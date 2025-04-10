using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Petzold.SpanTheCells
{   

    class BirdEventArg
    {
        public string Message { get; }
        public double TimeOfFligth { get; }
        public double MaxY { get; }
        public double Range { get;  }
        public BirdEventArg(string message, double timeOfFligth, double maxY, double range)
        {
            Message = message;
            TimeOfFligth = timeOfFligth;
            MaxY = maxY;
            Range = range; 
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

        public delegate void FallHandler(Bird sender, BirdEventArg e, StreamWriter writer);
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
        public void ReadFromTextBox(List<TextBox> textBoxes)
        {

            velocity = double.Parse(textBoxes[0].Text);
            angle = double.Parse (textBoxes[1].Text);
            m = double.Parse(textBoxes[2].Text);
            k = double.Parse(textBoxes[3].Text);
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
                        double maxY = Y.Max();
                        Notify?.Invoke(this, new BirdEventArg($"Тело упало. Время падения {timeOfFlight:F2} с," +
                            $" дальность броска {range:F2} м, максимальная высота {maxY:F2} м", timeOfFlight, maxY, range), writer);
                        
                        break;
                    }
                }
            }
        }
        public List<double> GetX()
        {
            return X;
        }
        public List<double> GetY()
        {
            return Y;
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
        private Canvas canv;

        public SpanTheCells()
        {
            Title = "Game";

            Grid grid = new Grid();
            grid.Margin = new Thickness(5);
            Content = grid;
            grid.ShowGridLines = true;

            ColumnDefinition coldef = new ColumnDefinition();
            coldef.Width = new GridLength(200, GridUnitType.Auto);
            grid.ColumnDefinitions.Add(coldef);

            coldef = new ColumnDefinition();
            coldef.Width = new GridLength(100, GridUnitType.Auto);
            grid.ColumnDefinitions.Add(coldef);

            coldef = new ColumnDefinition();
            coldef.Width = new GridLength(400, GridUnitType.Star);
            grid.ColumnDefinitions.Add(coldef);

            coldef = new ColumnDefinition();
            coldef.Width = new GridLength(400, GridUnitType.Auto);
            grid.ColumnDefinitions.Add(coldef);

            for (int i = 0; i < 5; i++)
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
                Grid.SetColumn(txtbox, 1);
                Grid.SetColumnSpan(txtbox, 4);
                textBoxes.Add(txtbox);
            }
            Button btn = new Button();
            btn.Content = "Запуск";
            btn.Margin = new Thickness(5);
            btn.IsDefault = true;
            btn.Click += buttonFly_Click;
            grid.Children.Add(btn);
            Grid.SetRow(btn, 4);
            Grid.SetColumn(btn, 1);
            grid.Children[1].Focus();

            txtOutput = new TextBox
            {
                Margin = new Thickness(5),
                IsReadOnly = true,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,  
                TextWrapping = TextWrapping.Wrap,
                AcceptsReturn = true, 
                MaxHeight = 400,
            };

            grid.Children.Add(txtOutput);
            Grid.SetColumn(txtOutput, 2);
            Grid.SetRowSpan(txtOutput, 5);

            canv = new Canvas
            {
                Width = 300,
                Height = 300,
                Background = Brushes.LightGray,
                Margin = new Thickness(5)
            };

            grid.Children.Add(canv);
            Grid.SetRow(canv, 0);
            Grid.SetColumn(canv, 3);
            Grid.SetRowSpan(canv, 5);

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

        void DrawLines(Canvas canv, List<double> X, List<double> Y)
        {
            canv.Children.Clear();

            double maxX = X.Max();
            double maxY = Y.Max();

            double k_x = (canv.ActualWidth) / (maxX);
            double k_y = (canv.ActualHeight) / (maxY);
            double k = Math.Min(k_x, k_y);


            for (int i = 0; i < X.Count - 1 & Y[i+1]>=0; i++)
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
            }
        }
    }
}