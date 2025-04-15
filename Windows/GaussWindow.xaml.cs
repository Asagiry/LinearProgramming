using LinearTools;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace LinearProgramming.Windows
{
    /// <summary>
    /// Interaction logic for GaussWindow.xaml
    /// </summary>
    public partial class GaussWindow : Window
    {

        /// <summary>
        /// Список равенств
        /// </summary>
        public List<LinearCondition> conditions { get; set; }
        /// <summary>
        /// Список базисных переменных
        /// </summary>
        public List<int> selectedX { get; set; }
        /// <summary>
        /// Переданная матрица
        /// </summary>
        public LinearTools.Matrix initialMatrix { get; set; }
        /// <summary>
        /// Холст вывода операций над матрицей
        /// </summary>
        public StackPanel log { get; set; }
        /// <summary>
        /// Холст выбора базисных переменных
        /// </summary>
        public Canvas pickX { get; set; }

        /// <summary>
        /// Основное окно чтобы после закрытия вернуться туда
        /// </summary>
        public MainWindow mainWindow { get; set; }
        public GaussWindow(List<LinearCondition> conditions, MainWindow mainWindow)
        {

            this.conditions = conditions;
            this.mainWindow = mainWindow;
            this.SizeChanged += FullScreenScaling;
            InitializeComponent();
            Initialize();
            Loaded += (object sender, RoutedEventArgs e) =>
            {
                this.WindowState = mainWindow.WindowState;
            };

        }



        #region TopButtons
        /// <summary>
        /// Логика 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void QuitButton_Click(object sender, RoutedEventArgs e)
        {
            mainWindow.Show();
            Close();
            Fraction.decimalFlag = true;

        }
        /// <summary>
        /// Логика минимизации окна
        /// </summary>
        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }
        /// <summary>
        /// Событие для перетаскивания окна
        /// </summary>
        private void Window_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            DragMove();
        }
        /// <summary>
        /// Событие для автоматической прокрутки вниз при расширении окна
        /// </summary>
        private void InputScroll_SizeChanged(object sender, SizeChangedEventArgs e)
        {

            InputScroll.ScrollToEnd();
        }

        private void FullScreenButton_Click(object sender, RoutedEventArgs e)
        {
            if (WindowState != WindowState.Maximized)
            {
                WindowState = WindowState.Maximized;
                mainWindow.WindowState = WindowState;
            }
            else
            {
                WindowState = WindowState.Normal;
                mainWindow.WindowState = WindowState;
            }
        }
        private void FullScreenScaling(object sender, SizeChangedEventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {

                // Переход в полноэкранный режим
                double scaleX = ActualWidth / 1400.0; // 1400 - исходная ширина окна
                double scaleY = ActualHeight / 800.0; // 800 - исходная высота окна

                // Увеличиваем пропорции для InputBorder
                InputBorder.Width = InputBorder.Width * scaleX + (255 * scaleX - 250);
                InputBorder.Height = InputBorder.Height * scaleY + (55 * scaleY - 50);

                // Увеличиваем только длину для InputLabel
                TaskBorder.Width = TaskBorder.Width * scaleX + (255 * scaleX - 250);

                // Увеличиваем только высоту для ActionsBorder
                ActionsBorder.Height = ActionsBorder.Height * scaleY + (110 * scaleY - 105);


                progressBar.Width = progressBar.Width * scaleX;
            }
            else
            {
                TaskBorder.Width = 1132;
                InputBorder.Height = 733;
                InputBorder.Width = 1132;
                ActionsBorder.Height = 678;
                progressBar.Width = 1388;
            }
        }

        #endregion TopButtons

        /// <summary>
        /// Инициализация окна
        /// </summary>
        private async void Initialize()
        {
            InputCanvas.Children.Clear();
            InputCanvas.Height = 0;
            InputCanvas.Width = 0;
            InputCanvas.Margin = new Thickness(5);

            initialMatrix = new LinearTools.Matrix(conditions);
            initialMatrix.drawMatrix();
            InputCanvas.Children.Add(initialMatrix);
            InputCanvas.Height += initialMatrix.Canvas.Height;

            InputCanvas.Width = Double.NaN;


            Label label = new Label();
            label.Content = "Проверка на совместность";
            label.Height = 50;
            label.FontSize = 20;
            InputCanvas.Children.Add(label);

            InputCanvas.Height += label.Height;


            List<int> check = new List<int>();
            for (int i = 0; i != conditions.Count; i++)
            {
                check.Add(i);
            }


            progressBar.Value = 0;
            progressBar.Visibility = Visibility.Visible;
            var progress = new Progress<int>(value => progressBar.Value = value);
            autoBasisButton.IsEnabled = false;
            LinearTools.Matrix endMatrix = await Utils.doGaussAsync(initialMatrix, check, progress);
            progressBar.Visibility = Visibility.Collapsed;
            autoBasisButton.IsEnabled = true;

            if (Utils.isRight(endMatrix))
            {
                label = new Label();
                label.Content = "Система несовместна";
                label.Height = 50;
                label.FontSize = 20;
                InputCanvas.Children.Add(label);
                InputCanvas.Height += label.Height;
                return;
            }
            label = new Label();
            label.Content = "Система совместна";
            label.Height = 50;
            label.FontSize = 20;
            InputCanvas.Children.Add(label);
            InputCanvas.Height += label.Height;

            label = new Label();
            label.Content = "Выберите базисные переменные";
            label.Height = 50;
            label.FontSize = 20;
            InputCanvas.Children.Add(label);
            InputCanvas.Height += label.Height;

            log = new StackPanel();
            selectedX = new List<int>();
            pickX = new Canvas();
            pickX.Height = 50;
            for (int i = 0; i != conditions[0].xAmount; i++)
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
                        if (InputCanvas.Children.Contains(log))
                        {
                            InputCanvas.Children.Remove(log);
                        }
                    }
                    else if (selectedX.Count < conditions.Count)
                    {
                        x.Background = new SolidColorBrush(Color.FromArgb(100, 255, 0, 0));
                        selectedX.Add(int.Parse(x.Uid));
                        if (selectedX.Count == conditions.Count)
                        {

                            startGauss(initialMatrix, selectedX, log);

                        }
                    }

                };
                Canvas.SetLeft(x, 5 + i * 60);
                pickX.Children.Add(x);
            }
            InputCanvas.Height += 50;
            InputCanvas.Children.Add(pickX);

            Utils.GaussOperation += gaussLog;

        }

        /// <summary>
        /// Запуск метода гаусса и вывод его в лог
        /// </summary>
        /// <param name="matrix">Матрица от которой запустить метод гаусса</param>
        /// <param name="basis">Список базисных переменных</param>
        /// <param name="log">Холст вывода изменений матрицы</param>
        private async void startGauss(LinearTools.Matrix matrix, List<int> basis, StackPanel log)
        {
            // Удаляем лог из InputCanvas и очищаем его содержимое
            if (InputCanvas.Children.Contains(log))
            {
                InputCanvas.Children.Remove(log);
            }
            log.Children.Clear();

            InputCanvas.Children.Add(log);
            basis.Sort();

            progressBar.Value = 0;
            progressBar.Visibility = Visibility.Visible;
            var progress = new Progress<int>(value => progressBar.Value = value);

            autoBasisButton.IsEnabled = false;
            LinearTools.Matrix endMatrix = await Utils.doGaussAsync(matrix, basis, progress);
            autoBasisButton.IsEnabled = true;
            progressBar.Visibility = Visibility.Collapsed;

            if (!Utils.isLineryIndependent(endMatrix, basis))
            {
                MessageBox.Show("Невозможно привести к ступенчатому виду");
            }
            else
            {
                MessageBox.Show("Успешно выполнено");
            }


        }

        /// <summary>
        /// Событие которое вызывается операциями над матрицей
        /// </summary>
        /// <param name="output">Полученная матрица</param>
        /// <param name="operation">Название операции</param>
        /// <param name="args">Аргументы операции</param>
        private void gaussLog(LinearTools.Matrix output, string operation, List<Fraction> args)
        {
            switch (operation)
            {
                case "minus":
                    {

                        Label label = new Label();
                        label.Content = string.Format("Вычитаем {0} строку из {1} строки умноженную на {2}", args[0], args[1], args[2]);
                        label.Height = 50;
                        label.FontSize = 20;

                        log.Children.Add(label);
                        log.Height += label.Height;
                        output.drawMatrix();
                        log.Children.Add(output);
                        log.Height += output.Canvas.Height;

                        InputCanvas.Height += log.Height;

                        break;
                    }
                case "divide":
                    {
                        Label label = new Label();
                        label.Content = string.Format("Делим {0} строчку на {2}", args[0], args[1], args[2]);
                        label.Height = 50;
                        label.FontSize = 20;

                        log.Children.Add(label);
                        log.Height += label.Height;
                        output.drawMatrix();
                        log.Children.Add(output);
                        log.Height += output.Canvas.Height;

                        InputCanvas.Height += log.Height;

                        break;
                    }
                case "swap":
                    {
                        Label label = new Label();
                        label.Content = string.Format("Меняем {0} строчку на {1}", args[0], args[1]);
                        label.Height = 50;
                        label.FontSize = 20;

                        log.Children.Add(label);
                        log.Height += label.Height;
                        output.drawMatrix();
                        log.Children.Add(output);
                        log.Height += output.Canvas.Height;

                        InputCanvas.Height += log.Height;
                        break;
                    }
            }
        }

        #region Buttons

        /// <summary>
        /// Событие переключения режима работы чисел
        /// Дробный меняется на десятичный и наоборот
        /// </summary>
        private void numberSwitchButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            switch (button.Content)
            {
                case "Дробный режим":
                    button.Content = "Десятичный режим";
                    Fraction.decimalFlag = false;
                    if (selectedX.Count == conditions.Count)
                    {

                        startGauss(initialMatrix, selectedX, log);

                    }
                    break;
                case "Десятичный режим":
                    button.Content = "Дробный режим";
                    Fraction.decimalFlag = true;
                    if (selectedX.Count == conditions.Count)
                    {

                        startGauss(initialMatrix, selectedX, log);

                    }
                    break;
            }
        }

        /// <summary>
        /// Событие автоматического выбора базиса
        /// Находит первый попавшийся линейно-независимый базис
        /// </summary>
        private async void autoBasisButton_Click(object sender, RoutedEventArgs e)
        {
            selectedX.Clear();
            Utils.ResetGaussOperation();
            progressBar.Value = 0;
            progressBar.Visibility = Visibility.Visible;
            autoBasisButton.IsEnabled = false;

            var progress = new Progress<int>(value => progressBar.Value = value);

            List<int> basisList = await Utils.AutoFindBasisAsync(initialMatrix, progress);
            progressBar.Visibility = Visibility.Collapsed;
            autoBasisButton.IsEnabled = true;
            if (basisList.Count > 0)
            {
                for (int j = 0; j != pickX.Children.Count; j++)
                {
                    if (basisList.Contains(int.Parse(pickX.Children[j].Uid)))
                    {
                        ((Label)pickX.Children[j]).Background = new SolidColorBrush(Color.FromArgb(100, 255, 0, 0));
                    }
                    else
                    {
                        ((Label)pickX.Children[j]).Background = Brushes.Transparent;
                    }
                }
                selectedX = basisList;
                Utils.GaussOperation += gaussLog;
                startGauss(initialMatrix, selectedX, log);
            }
            else
            {
                Utils.GaussOperation += gaussLog;
                MessageBox.Show("Невозможно привести к ступенчатому виду");
            }
        }


        #endregion Buttons

        //Сделать более привлекательные MessageBox в AutoBasisButton 
        //И в startGauss

    }
}
