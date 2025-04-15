using LinearTools;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace LinearProgramming.Windows
{
    /// <summary>
    /// Interaction logic for BasisSelection.xaml
    /// </summary>
    public partial class BasisSelection : Window
    {
        public int xAmount { get; set; }
        public int conditionsCount { get; set; }
        public List<int> selectedX { get; set; }
        public BasisSelection(int xAmount, int conditionsCount)
        {
            InitializeComponent();
            this.xAmount = xAmount;
            this.conditionsCount = conditionsCount;
            help.Content = "Вы можете выбрать " + Math.Min(xAmount, conditionsCount) + " переменных";

            Initialize();
        }
        public void Initialize()
        {
            selectedX = new List<int>();
            for (int i = 0; i != xAmount; i++)
            {
                Label x = new Label();
                x.Content = "x" + Utils.makeLowerIndex(i + 1);
                x.Height = 50;
                x.Width = 50;
                x.FontSize = 30;
                x.BorderBrush = Brushes.Black;
                x.BorderThickness = new Thickness(1);
                x.Uid = i.ToString();
                x.MouseLeftButtonDown += (sender, e) =>
                {
                    if (selectedX.Contains(int.Parse(x.Uid)))
                    {
                        x.Background = Brushes.Transparent;
                        selectedX.Remove(int.Parse(x.Uid));
                        confirm.IsEnabled = false;
                    }
                    else if (selectedX.Count < conditionsCount)
                    {
                        x.Background = new SolidColorBrush(Color.FromArgb(100, 255, 0, 0));
                        selectedX.Add(int.Parse(x.Uid));
                        if (selectedX.Count == Math.Min(xAmount, conditionsCount))
                        {

                            confirm.IsEnabled = true;

                        }
                    }

                };
                Canvas.SetTop(x, 5 + (i / 6) * 55);
                Canvas.SetLeft(x, 5 + (i % 6) * 62);
                InputCanvas.Children.Add(x);
            }


        }

        private void confirm_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

    }
}
