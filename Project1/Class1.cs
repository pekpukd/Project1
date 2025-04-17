using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
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

        public void ReadInputData(string inputFile)
        {
            string[] lines = File.ReadAllLines(inputFile);
            velocity = double.Parse(lines[0]);
            angle = double.Parse(lines[1]);
            m = double.Parse(lines[2]);
            k = double.Parse(lines[3]);
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
                Velocity_X.Add(velocity * Math.Cos(angle));
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
            grid.ShowGridLines = false;

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

            DockPanel dock = new DockPanel();
            Content = dock;

            Menu menu = new Menu();
            dock.Children.Add(menu);
            DockPanel.SetDock(menu, Dock.Top);
            dock.Children.Add(grid);
            DockPanel.SetDock(grid, Dock.Bottom);

            // Создание меню File.
            MenuItem itemFile = new MenuItem();
            itemFile.Header = "_File";
            menu.Items.Add(itemFile);

            MenuItem itemNew = new MenuItem();
            itemNew.Header = "_New";
            itemNew.Click += UnimplementedOnClick;
            itemFile.Items.Add(itemNew);

            MenuItem itemOpen = new MenuItem();
            itemOpen.Header = "_Open";
            itemOpen.Click += UnimplementedOnClick;
            itemFile.Items.Add(itemOpen);

            MenuItem itemSave = new MenuItem();
            itemSave.Header = "_Save";
            itemSave.Click += UnimplementedOnClick;
            itemFile.Items.Add(itemSave);

            itemFile.Items.Add(new Separator()); //рисует горизонтальную разделительную линию
            MenuItem itemExit = new MenuItem();
            itemExit.Header = "E_xit";
            itemExit.Click += ExitOnClick;
            itemFile.Items.Add(itemExit);

            // Создание меню Window.
            MenuItem itemWindow = new MenuItem();
            itemWindow.Header = "_Window";
            menu.Items.Add(itemWindow);

            MenuItem itemTaskbar = new MenuItem();
            itemTaskbar.Header = "_Show in Taskbar";
            itemTaskbar.IsCheckable = true;   //может быть отмечен галочкой
            itemTaskbar.IsChecked = ShowInTaskbar;
            itemTaskbar.Click += TaskbarOnClick;
            itemWindow.Items.Add(itemTaskbar);

            MenuItem itemSize = new MenuItem();
            itemSize.Header = "Size to _Content";
            itemSize.IsCheckable = true;
            itemSize.IsChecked = SizeToContent == SizeToContent.WidthAndHeight;
            itemSize.Checked += SizeOnCheck;
            itemSize.Unchecked += SizeOnCheck;
            itemWindow.Items.Add(itemSize);

            MenuItem itemResize = new MenuItem();
            itemResize.Header = "_Resizable";
            itemResize.IsCheckable = true;
            itemResize.IsChecked = ResizeMode == ResizeMode.CanResize;
            itemResize.Click += ResizeOnClick;
            itemWindow.Items.Add(itemResize);

            MenuItem itemTopmost = new MenuItem();
            itemTopmost.Header = "_Topmost";
            itemTopmost.IsCheckable = true;
            itemTopmost.IsChecked = Topmost;
            itemTopmost.Checked += TopmostOnCheck;
            itemTopmost.Unchecked += TopmostOnCheck;
            itemWindow.Items.Add(itemTopmost);


        }

        void UnimplementedOnClick(object sender, RoutedEventArgs args)    
                                                                          
        {
            MenuItem item = sender as MenuItem;
            if(item.Header == "_Open")
            {
                OnOpen(sender, args);
            }
            string strItem = item.Header.ToString().Replace("_", "");
            MessageBox.Show("The " + strItem +
                " option has not yet  been implemented", Title);
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
        void OnOpen(object sender, RoutedEventArgs args)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.CheckFileExists = true;

            if ((bool)dlg.ShowDialog(this))
            {
                try
                {
                    Bird bird = new Bird();
                    bird.ReadInputData(dlg.FileName);
                }
                catch (Exception exc)
                {
                    MessageBox.Show(exc.Message, Title);
                }
            }
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