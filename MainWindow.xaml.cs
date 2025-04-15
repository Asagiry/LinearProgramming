using LinearProgramming.Windows;
using LinearTools;
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace LinearProgramming
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>



    ///Основное окно
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Заданная функция
        /// </summary>
        public LinearFunction function { get; set; }
        /// <summary>
        /// Заданные условия
        /// </summary>
        public List<LinearCondition> conditionsInput { get; set; }
        /// <summary>
        /// Выбранные условия
        /// </summary>
        public List<LinearCondition> selectedConditions { get; set; }
        /// <summary>
        /// Количество переменных
        /// </summary>
        public int xAmount { get; set; }
        /// <summary>
        /// Количество условий
        /// </summary>
        public int conditionsAmount { get; set; }
        public MainWindow()
        {

            this.SizeChanged += FullScreenScaling;
            InitializeComponent();

            randomGenerateButton_Click(null, null);
        }
        #region Unimportant
        /// <summary>
        /// Выход из приложения
        /// </summary>
        private void QuitButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
        /// <summary>
        /// Минимизировать окно
        /// </summary>
        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }
        /// <summary>
        /// Расширить окно
        /// </summary>
        private void FullScreenButton_Click(object sender, RoutedEventArgs e)
        {
            if (WindowState != WindowState.Maximized || WindowStyle != WindowStyle.None)
            {
                WindowState = WindowState.Maximized;
            }
            else
            {
                WindowState = WindowState.Normal;
            }
        }
        /// <summary>
        /// Функция расширения элементов окна из за изменения размера
        /// </summary>
        private void FullScreenScaling(object sender, SizeChangedEventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                // Переход в полноэкранный режим
                double scaleX = ActualWidth / 1400; // 1400 - исходная ширина окна
                double scaleY = ActualHeight / 800; // 800 - исходная высота окна
                // Увеличиваем пропорции для InputBorder
                InputBorder.Width = InputBorder.Width * scaleX + (510 * scaleX - 505);
                InputBorder.Height = InputBorder.Height * scaleY + (55 * scaleY - 50);

                // Увеличиваем только длину для InputLabel
                InputLabel.Width = InputLabel.Width * scaleX + (510 * scaleX - 505);

                // Увеличиваем только высоту для ActionsBorder
                ActionsBorder.Height = ActionsBorder.Height * scaleY + (110 * scaleY - 105);

                // Увеличиваем только высоту для TasksBorder
                TasksBorder.Height = TasksBorder.Height * scaleY + (55 * scaleY - 50);

                progressBar.Width = progressBar.Width * scaleX;
            }
            else
            {
                // Возвращение к стандартному виду окна
                InputBorder.Height = 733;
                InputBorder.Width = 877;
                InputLabel.Width = 877;
                ActionsBorder.Height = 678;
                TasksBorder.Height = 733;
                progressBar.Width = 1388;
            }
        }
        /// <summary>
        /// Событие для перемещения окна
        /// </summary>
        private void mainWindow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }
        /// <summary>
        /// Считывание с файла
        /// </summary>
        private void readFileButton_Click(object sender, RoutedEventArgs e)
        {
            // Открываем диалоговое окно для выбора файла
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
                Title = "Выберите JSON файл"
            };

            if (!(openFileDialog.ShowDialog() == true))
            {
                return;
            }

            List<List<string>> jsonMatrix = new List<List<string>>();

            try
            {
                string jsonFilePath = openFileDialog.FileName;
                string jsonString = File.ReadAllText(jsonFilePath);

                // Используем Newtonsoft.Json для десериализации
                jsonMatrix = JsonConvert.DeserializeObject<List<List<string>>>(jsonString);
            }
            catch (Exception)
            {
                MessageBox.Show("Файл пуст или содержит неверные данные.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            bool min = true;
            xAmount = jsonMatrix[0].Count - 1;
            int cnd = jsonMatrix.Count - 1;
            if (jsonMatrix[0].Last() == "True" || jsonMatrix[0].Last() == "False")
            {
                min = jsonMatrix[0].Last() == "True";
                xAmount = jsonMatrix[0].Count - 2;
                cnd = jsonMatrix.Count - 1;
            }


            xAmountTextBox.TextChanged -= xAmount_TextChanged;
            xAmountTextBox.Text = xAmount.ToString();
            xAmountTextBox.TextChanged += xAmount_TextChanged;




            List<Fraction> cList = new List<Fraction>();
            for (int i = 0; i != xAmount + 1; i++)
            {
                cList.Add(Fraction.Parse(jsonMatrix[0][i]));
            }

            createInput();
            createFunction(cList, min);

            createConditionsSpace();


            for (int i = 1; i != cnd + 1; i++)
            {
                List<Fraction> condition = new List<Fraction>();
                for (int j = 0; j != xAmount + 1; j++)
                {
                    condition.Add(Fraction.Parse(jsonMatrix[i][j]));
                }
                if (openFileDialog.FileName.Contains("Graphic"))
                {
                    ConditionType conditionType = (ConditionType)Enum.Parse(typeof(ConditionType), jsonMatrix[i][xAmount + 1]);
                    createCondition(condition, conditionType);
                }
                else
                {
                    createCondition(condition);
                }
            }


            if (addConditionButton != null && xAmount < conditionsAmount)
            {
                gaussButton.IsEnabled = false;
            }
            else
            {

                gaussButton.IsEnabled = true;

            }
        }
        /// <summary>
        /// Сохранение в файл
        /// </summary>
        private void saveFileButton_Click(object sender, RoutedEventArgs e)
        {
            // Открываем диалоговое окно для сохранения файла
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
                Title = "Сохранить JSON файл"
            };

            if (!(saveFileDialog.ShowDialog() == true))
            {
                return;
            }

            // Создаем матрицу для сохранения
            List<List<string>> jsonMatrix = new List<List<string>>();

            // Сохраняем первую строку (целевая функция)
            List<string> functionRow = new List<string>();
            foreach (var fraction in function.GetRawData())
            {
                functionRow.Add(fraction.ToString()); // Конвертируем Fraction в строку

            }
            functionRow.Add(function.min.ToString());
            jsonMatrix.Add(functionRow);

            // Сохраняем остальные строки (ограничения)
            foreach (var condition in conditionsInput)
            {
                List<string> conditionRow = new List<string>();

                foreach (Fraction fraction in condition.getData())
                {
                    conditionRow.Add(fraction.ToString()); // Конвертируем Fraction в строку
                }
                if (saveFileDialog.FileName.Contains("Graphic"))
                    conditionRow.Add(condition.conditionType.ToString());
                jsonMatrix.Add(conditionRow);
            }

            try
            {
                string jsonFilePath = saveFileDialog.FileName;
                string jsonString = JsonConvert.SerializeObject(jsonMatrix, Formatting.Indented);
                File.WriteAllText(jsonFilePath, jsonString);
                MessageBox.Show("Данные успешно сохранены.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при сохранении файла: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        /// <summary>
        /// Обработка ввода
        /// </summary>
        /// <returns>True если все введено правильно<br></br>
        /// False если есть ошибки</returns>
        public bool HandleInput(bool isSupportConditionTypes = false)
        {
            bool validated = true;
            for (int i = 0; i != conditionsAmount; i++)
            {
                try
                {
                    List<Fraction> condition = conditionsInput[i].getData();
                    if (!isSupportConditionTypes)
                    {
                        if (conditionsInput[i].conditionType != ConditionType.Equal)
                        {
                            validated = false;
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    return false;
                }
            }
            if (!validated)
            {
                MessageBox.Show("Данный режим не поддерживает разные типы условий\n" +
                                "Все условия будут равенствами");
            }
            return true;
        }
        #endregion Uninmortant

        #region inputChanged
        /// <summary>
        /// Событие отвечающее за изменение количества переменных<br></br>
        /// При изменении все условия и сама функция обнуляются
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void xAmount_TextChanged(object sender, TextChangedEventArgs e)
        {
            int x;
            if (!int.TryParse((sender as TextBox).Text, out x))
                return;
            if (x > 16 || x <= 0)
                return;
            xAmount = x;

            createInput();
            createFunction();
            if (conditionsAmountTextBox != null)
            {
                conditionsAmount_TextChanged(conditionsAmountTextBox, e);
            }
        }
        /// <summary>
        /// Событие отвечающее за изменение количества равенств<br></br>
        /// При изменении функция остается такой же а все условия стираются
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void conditionsAmount_TextChanged(object sender, TextChangedEventArgs e)
        {
            int x;
            if (!int.TryParse((sender as TextBox).Text, out x))
                return;
            if (x > xAmount || x < 0)
                return;



            createConditionsSpace();
            for (int i = 0; i != x; i++)
            {
                createCondition();
            }

        }
        #endregion inputChanged

        #region createBaseInput
        /// <summary>
        /// Функиця задания начального canvas
        /// </summary>
        private void createInput()
        {
            conditionsInput = new List<LinearCondition>();
            selectedConditions = new List<LinearCondition>();
            InputCanvas.Children.Clear();
            InputCanvas.Height = 15;
            InputCanvas.Width = 15;


        }
        /// <summary>
        /// Функция отрисовки фукнции
        /// </summary>
        private void createFunction(List<Fraction> cList = null, bool min = true)
        {
            function = new LinearFunction(xAmount, cList, min);
            Canvas.SetLeft(function, 5);
            Canvas.SetTop(function, 5);
            InputCanvas.Children.Add(function);
            InputCanvas.Height += 50;
            InputCanvas.Width += 110 + (xAmount + 1) * 80;


        }
        /// <summary>
        /// Функция создания места под равенства
        /// </summary>
        private void createConditionsSpace()
        {
            InputCanvas.Children.Remove(conditionsBorder);
            conditionsInput.Clear();
            conditionsAmount = 0;
            InputCanvas.Height = 65;

            conditionsBorder.BorderBrush = Brushes.Black;
            conditionsBorder.BorderThickness = new Thickness(1, 1, 1, 0);
            conditionsBorder.Width = 115 + (xAmount) * 80;
            conditionsBorder.Height = 0;
            Canvas.SetLeft(conditionsBorder, 58);
            InputCanvas.Children.Add(conditionsBorder);


            Canvas conditionsCanvas = new Canvas();
            conditionsCanvas.Height = double.NaN;
            conditionsBorder.Child = conditionsCanvas;


            Canvas.SetTop(conditionsBorder, 60);
        }
        /// <summary>
        /// Функция создания равенства
        /// </summary>
        private void createCondition(List<Fraction> aList = null, ConditionType conditionType = ConditionType.Equal)
        {
            Canvas conditionsCanvas = conditionsBorder.Child as Canvas;

            LinearCondition condition = new LinearCondition(xAmount, aList, conditionType);
            condition.SelectCondition += selectConditionEvent;
            condition.RemoveCondition += removeConditionEvent;

            Canvas.SetTop(condition, 50 * conditionsAmount);

            InputCanvas.Height += 50;
            conditionsBorder.Height += 50;

            conditionsInput.Add(condition);
            conditionsCanvas.Children.Add(condition);
            conditionsAmount += 1;
        }
        /// <summary>
        /// Удаление равенства
        /// </summary>
        /// <param name="condition"></param>
        private void removeCondition(LinearCondition condition)
        {
            Canvas conditionCanvas = conditionsBorder.Child as Canvas;
            conditionCanvas.Children.Remove(condition);
            selectedConditions.Remove(condition);
            conditionsInput.Remove(condition);


            conditionsAmount -= 1;
            conditionsAmountTextBox.TextChanged -= conditionsAmount_TextChanged;
            conditionsAmountTextBox.Text = conditionsAmount.ToString();
            conditionsAmountTextBox.TextChanged += conditionsAmount_TextChanged;

            InputCanvas.Height -= 50;
            conditionsBorder.Height -= 50;

            for (int i = 0; i != conditionsAmount; i++)
            {
                Canvas.SetTop(conditionCanvas.Children[i], 50 * i);
            }
            if (conditionsAmount <= xAmount)
            {
                gaussButton.IsEnabled = true;
            }

        }

        #endregion createBaseInput

        #region Events
        /// <summary>
        /// Ивент выбора условия
        /// </summary>
        private void selectConditionEvent(object sender, EventArgs e)
        {
            LinearCondition selected = (LinearCondition)sender;

            if (selectedConditions.Contains(selected))
            {
                selected.background.Opacity = 0;
                selectedConditions.Remove(selected);
            }
            else
            {
                selected.background.Opacity = 0.15;
                selectedConditions.Add(selected);
            }

        }
        /// <summary>
        /// Ивент удаления условия
        /// </summary>
        private void removeConditionEvent(object sender, EventArgs e)
        {
            removeCondition((LinearCondition)sender);
        }
        #endregion

        #region Buttons
        /// <summary>
        /// Обработка добавления условий
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void addConditionButton_Click(object sender, RoutedEventArgs e)
        {
            conditionsAmountTextBox.TextChanged -= conditionsAmount_TextChanged;
            conditionsAmountTextBox.Text = (conditionsAmount + 1).ToString();
            conditionsAmountTextBox.TextChanged += conditionsAmount_TextChanged;
            createCondition();
            if (conditionsAmount > xAmount)
            {
                gaussButton.IsEnabled = false;
            }
            else
            {
                gaussButton.IsEnabled = true;
            }

        }
        /// <summary>
        /// Обработка удаления условий
        /// </summary>
        private void removeConditionButton_Click(object sender, RoutedEventArgs e)
        {

            for (int i = selectedConditions.Count - 1; i >= 0; i--)
            {
                removeCondition(selectedConditions[i]);
            }
            selectedConditions.Clear();

        }
        /// <summary>
        /// Обработка случайного заполнения
        /// </summary>
        private async void randomGenerateButton_Click(object sender, RoutedEventArgs e)
        {

            Random random = new Random();

            List<Fraction> randomNumbers = new List<Fraction>();

            for (int j = 0; j != xAmount + 1; j++)
            {
                randomNumbers.Add(new Fraction(random.Next(0, 5)));
            }

            createInput();
            createFunction(randomNumbers);

            int cnd = conditionsAmount;
            createConditionsSpace();

            randomGenerateButton.IsEnabled = false;
            progressBar.Value = 0;
            progressBar.Visibility = Visibility.Visible;
            for (int i = 0; i != cnd; i++)
            {
                randomNumbers = new List<Fraction>();

                for (int j = 0; j != xAmount + 1; j++)
                {
                    randomNumbers.Add(new Fraction(random.Next(0, 50)));
                }

                createCondition(randomNumbers);
                progressBar.Value += 100.0 / (double)cnd;
                await Task.Delay(50);
            }
            randomGenerateButton.IsEnabled = true;
            progressBar.Visibility = Visibility.Collapsed;


            if (addConditionButton != null && xAmount > conditionsAmount)
            {
                addConditionButton.IsEnabled = true;
            }
        }

        #endregion Buttons

        #region MenuButtons
        /// <summary>
        /// Открытие окна Метода гаусса
        /// </summary>
        private void gaussButton_Click(object sender, RoutedEventArgs e)
        {
            if (HandleInput())
            {
                Fraction.decimalFlag = true;
                GaussWindow gaussWindow = new GaussWindow(conditionsInput, this);
                gaussWindow.Show();
                this.Hide();
            }
        }
        /// <summary>
        /// Открытие окна Симплекс метода
        /// </summary>
        private void simplexButton_Click(object sender, RoutedEventArgs e)
        {
            if (HandleInput())
            {
                Fraction.decimalFlag = true;
                Utils.ResetSimplexOperation();
                SimplexWindow simplexWindow = new SimplexWindow(this, function, conditionsInput);
                simplexWindow.Show();
                this.Hide();
            }
        }
        /// <summary>
        /// Открытие окна графического метода
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void graphicButton_Click(object sender, RoutedEventArgs e)
        {
            if (HandleInput(true))
            {
                Fraction.decimalFlag = true;
                GraphicWindow graphicWindow = new GraphicWindow(this, function, conditionsInput);
                graphicWindow.Show();

            }
        }
        /// <summary>
        /// Открытие окна справки
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HelpButton_Click(object sender, RoutedEventArgs e)
        {
            HelpWindow helpWindow = new HelpWindow(this);
            helpWindow.Show();

        }


        #endregion MenuButtons

    }
}  //TODO customMessageBox, IntegerGraphic, Dvoistvennaya, Test, SIMPLEX WITH <= >=
