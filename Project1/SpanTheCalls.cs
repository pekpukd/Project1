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
using System.Threading.Tasks;
namespace Project1
{   
    public partial class SpanTheCells : Window
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

            MenuItem itemWindow = new MenuItem();
            itemWindow.Header = "_Window";
            menu.Items.Add(itemWindow);

            MenuItem itemTaskbar = new MenuItem();
            itemTaskbar.Header = "_Show in Taskbar";
            itemTaskbar.IsCheckable = true;   
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
    }
}