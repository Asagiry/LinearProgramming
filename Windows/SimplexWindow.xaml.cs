using LinearTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LinearProgramming.Windows
{
    /// <summary>
    /// Interaction logic for SimplexWindow.xaml
    /// </summary>
    public partial class SimplexWindow : Window
    {
        /// <summary>
        /// Список условий типа LinearCondition
        /// </summary>
        public List<LinearCondition> conditions { get; set; }
        /// <summary>
        /// Функция типа LinearFunction
        /// </summary>
        public LinearFunction function { get; set; }
        /// <summary>
        /// Основное окно чтобы вернуться туда
        /// </summary>
        public MainWindow mainWindow { get; set; }
        /// <summary>
        /// Список всех таблиц что есть на экране
        /// </summary>
        public List<SimplexTable> tables { get; set; }
        /// <summary>
        /// Количество переменных в задаче
        /// </summary>
        int xAmount { get; set; }
        /// <summary>
        /// Количество условий в задаче
        /// </summary>
        int conditionsCount { get; set; }
        /// <summary>
        /// Массив базисных переменных
        /// </summary>
        public int[] basisList { get; set; }
        /// <summary>
        /// Находим ли мы сначала искуственный базис
        /// </summary>
        bool findAritificial = false;
        /// <summary>
        /// Новый опорный для шага назад
        /// </summary>
        List<int> newMainElement = null;


        /// <summary>
        /// Инициализация окна симплекс метода
        /// </summary>
        /// <param name="mainWindow">Основное окно, куда нужно будет вернуться при закрытии</param>
        /// <param name="function">Функция к минимуму</param>
        /// <param name="conditions">Список равенств данной задачи</param>
        public SimplexWindow(MainWindow mainWindow, LinearFunction function, List<LinearCondition> conditions)
        {
            InitializeComponent();

            this.mainWindow = mainWindow;
            this.function = function;
            this.conditions = conditions;
            this.SizeChanged += FullScreenScaling;
            Loaded += (object sender, RoutedEventArgs e) =>
            {
                this.WindowState = mainWindow.WindowState;
            };

            basisList = new int[conditions.Count];

            Utils.SimplexOperation += simplexLog;
            Utils.PreviousStep += PreviousStepEvent;
            BasisSelection basisSelection = new BasisSelection(conditions[0].xAmount, conditions.Count);
            if (basisSelection.ShowDialog() == true)
            {
                basisList = basisSelection.selectedX.ToArray();
                findAritificial = false;
                Initialize();
            }
            else
            {
                findAritificial = true;
                Initialize();
            }

        }

        #region unimportant
        /// <summary>
        /// Закрыть окно
        /// </summary>
        private void QuitButton_Click(object sender, RoutedEventArgs e)
        {
            mainWindow.Show();
            Close();

            Fraction.decimalFlag = false;
            Utils.AutoSimplex = true;


        }
        /// <summary>
        /// Минимизация окна
        /// </summary>
        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {

            this.WindowState = WindowState.Minimized;
            InputCanvas.Height = 0;

        }
        /// <summary>
        /// Расширение окна
        /// </summary>
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
        /// <summary>
        /// Переопределение размеров элементов fullScreen режима
        /// </summary>
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


                //progressBar.Width = progressBar.Width * scaleX;
            }
            else
            {
                TaskBorder.Width = 1132;
                InputBorder.Height = 733;
                InputBorder.Width = 1132;
                ActionsBorder.Height = 678;
                //progressBar.Width = 1388;
            }
        }
        /// <summary>
        /// Событие для перетаскивания мыши
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }
        /// <summary>
        /// Переопределение размера InputCanvas чтобы он бесконечно не увеличивался
        /// </summary>
        private void ScrollViewer_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            double minHeight = 0;
            double minWidth = 0;
            foreach (var child in InputCanvas.Children)
            {
                // Проверяем, есть ли у элемента свойство Height
                if (child is FrameworkElement frameworkElement)
                {
                    minHeight += frameworkElement.Height; // Добавляем высоту элемента к общей
                    if (minWidth < frameworkElement.Width)
                        minWidth = frameworkElement.Width;
                }
            }
            if (minHeight < InputCanvas.Height)
            {
                InputCanvas.SizeChanged -= ScrollViewer_SizeChanged;
                InputCanvas.Height = minHeight;
                InputCanvas.Width = minWidth;
                InputCanvas.SizeChanged += ScrollViewer_SizeChanged;
            }
        }
        /// <summary>
        /// Лог для отслеживания операций симплекс метода
        /// </summary>
        /// <param name="operation">Название операции</param>
        /// <param name="args">Аргументы данной операции</param>
        private void simplexLog(string operation, List<Fraction> args)
        {

            switch (operation)
            {
                case "mainElement":
                    {
                        Label label = new Label();
                        label.Content = string.Format("строка = {0} столбец = {1}", args[0], args[1]);
                        label.Height = 50;
                        label.FontSize = 20;
                        label.Uid = "log";
                        InputCanvas.Children.Add(label);
                        InputCanvas.Height += label.Height;
                        break;
                    }
                case "end":
                    {
                        Label label = new Label();
                        label.Content = string.Format("Финальный план = ({0}) , F* = {1}", string.Join(" ; ", args), function.GetExtremium(args));
                        label.Height = 50;
                        label.FontSize = 20;
                        label.Uid = "end";
                        InputCanvas.Children.Add(label);
                        InputCanvas.Height += label.Height;
                        if (InputCanvas.Width <= label.Width)
                        {
                            InputCanvas.Width = label.Width + 50;
                        }
                        break;
                    }
                case "basis":
                    {
                        Label label = new Label();
                        label.Content = "Изначальный базис неправильный";
                        label.Height = 50;
                        label.FontSize = 20;
                        label.Uid = "end";
                        InputCanvas.Children.Add(label);
                        InputCanvas.Height += label.Height;
                        break;
                    }
                case "wrong":
                    {
                        Label label = new Label();
                        label.Content = "Задача неразрешима";
                        label.Height = 50;
                        label.FontSize = 20;
                        label.Uid = "end";
                        InputCanvas.Children.Add(label);
                        InputCanvas.Height += label.Height;
                        break;
                    }
                case "idleStep":
                    {
                        Label label = new Label();
                        label.Content = string.Format("Из задачи была удалена строчка {0}", args[args.Count - 1]);
                        label.Height = 50;
                        label.FontSize = 20;
                        label.Uid = "log";
                        InputCanvas.Children.Add(label);
                        InputCanvas.Height += label.Height;
                        break;
                    }

            }
        }
        #endregion unimportant

        #region Buttons
        /// <summary>
        /// Логика переключения десятичного режима в дробный
        /// </summary>
        private void numberSwitchButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            switch (button.Content)
            {
                case "Дробный режим":
                    {
                        button.Content = "Десятичный режим";
                        Fraction.decimalFlag = false;
                        Initialize();
                        break;
                    }
                case "Десятичный режим":
                    {
                        button.Content = "Дробный режим";
                        Fraction.decimalFlag = true;
                        Initialize();
                        break;
                    }
            }
        }
        /// <summary>
        /// Логика переключения автоматического режима и пошагового
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void modeSwitchButton_Copy_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            switch (button.Content)
            {
                case "Ручной режим":
                    {
                        Utils.AutoSimplex = false;
                        button.Content = "Автоматический режим";
                        Initialize();
                        break;
                    }
                case "Автоматический режим":
                    {

                        Utils.AutoSimplex = true;
                        previousButton.IsEnabled = false;
                        button.Content = "Ручной режим";
                        Initialize();
                        break;
                    }
            }

        }
        /// <summary>
        /// Логика кнопки выбора базиса
        /// </summary>
        private void basisButton_Click(object sender, RoutedEventArgs e)
        {
            BasisSelection basisSelection = new BasisSelection(conditions[0].xAmount, conditions.Count);
            if (basisSelection.ShowDialog() == true)
            {
                basisList = basisSelection.selectedX.ToArray();
                findAritificial = false;
                Initialize();
            }
            else
            {
                findAritificial = true;
                Initialize();
            }
        }
        /// <summary>
        /// Логика кнопки шага назад
        /// </summary>
        private void PreviousButton_Click(object sender, RoutedEventArgs e)
        {
            newMainElement = null;
            PreviousStep(true);
        }

        #endregion

        #region mainLogic
        /// <summary>
        /// Инициалицазия основного окна с последующим запуском симплекс метода<br></br>
        /// В зависимости от <value>findAritificial</value> запускает либо искуственный базис с последующим вычислением<br></br>
        /// Либо симплекс метод с выбранным базисом изначально
        /// </summary>
        private async void Initialize()
        {
            InputCanvas.Children.Clear();
            InputCanvas.Height = 0;
            InputCanvas.Width = 0;
            InputCanvas.Margin = new Thickness(5);
            tables = new List<SimplexTable>();
            previousButton.IsEnabled = false;

            SimplexTable simplexTable = null;

            if (findAritificial)
                simplexTable = Utils.FormToAritificialBasis(function, conditions);
            else
            {
                try
                {
                    simplexTable = new SimplexTable(conditions, function, basisList);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    return;
                }
            }


            simplexTable.drawSimplexTable();

            tables.Add(simplexTable);

            InputCanvas.Children.Add(simplexTable);
            InputCanvas.Height += simplexTable.Canvas.Height;
            InputCanvas.Width += simplexTable.Canvas.Width;


            if (findAritificial)
                await RunSimplexWithArtificial(simplexTable);
            else
                 if (Utils.CheckSimplexTable(simplexTable))
                await RunSimplex(simplexTable);


        }
        /// <summary>
        /// Делает одну итерацию симплекс таблицы
        /// </summary>
        /// <param name="simplexTable">Симплекс таблица от которой нужно сделать итерацию</param>
        /// <returns></returns>
        private async Task<SimplexTable> startIteration(SimplexTable simplexTable)
        {
            SimplexTable nextSimplexTable = await Utils.DoSimplexIteration(simplexTable, newMainElement);
            if (newMainElement != null)
            {
                newMainElement = null;
            }
            if (nextSimplexTable != null)
            {
                nextSimplexTable.drawSimplexTable();
                InputCanvas.Children.Add(nextSimplexTable);
                InputCanvas.Height += simplexTable.Canvas.Height;

                if (InputCanvas.Width <= simplexTable.Canvas.Width)
                {
                    InputCanvas.Width = simplexTable.Canvas.Width + 25;
                }
            }
            return nextSimplexTable;
        }
        /// <summary>
        /// Запускает симплекс метод от заданной симплекс таблицы
        /// </summary>
        /// <param name="simplexTable">Симплекс таблица которую нужно прогнать через метод</param>
        /// <returns></returns>
        private async Task RunSimplex(SimplexTable simplexTable)
        {

            SimplexTable nextSimplexTable = await startIteration(simplexTable);

            while (nextSimplexTable != null)
            {
                if (!Utils.AutoSimplex)
                {
                    tables.Add(nextSimplexTable);
                    previousButton.IsEnabled = true;
                }
                nextSimplexTable = await startIteration(nextSimplexTable);
            }

        }
        /// <summary>
        /// Запускает симплекс метод от заданной симплекс таблицы с нахождением искуственного базиса
        /// </summary>
        /// <param name="artificalBasis">Симплекс таблица в которой нужно найти искусственный базис а потом решить ее</param>
        /// <returns></returns>
        private async Task RunSimplexWithArtificial(SimplexTable artificalBasis)
        {
            int[] localBasis = new int[function.xAmount + conditions.Count];

            xAmount = artificalBasis.xAmount;
            conditionsCount = artificalBasis.conditions.Count;

            artificalBasis = await Utils.DoSimplexIteration(artificalBasis, newMainElement);
            if (artificalBasis != null)
                localBasis = artificalBasis.basisList;
            else
                return;
            while (Utils.ContainsArtificial(artificalBasis.basisList, xAmount, conditions.Count))
            {

                artificalBasis.drawSimplexTable();

                InputCanvas.Children.Add(artificalBasis);
                InputCanvas.Height += artificalBasis.Canvas.Height;
                if (InputCanvas.Width <= artificalBasis.Canvas.Width)
                {
                    InputCanvas.Width = artificalBasis.Canvas.Width + 25;
                }


                tables.Add(artificalBasis);

                if (!Utils.AutoSimplex && tables.Count > 1)
                    previousButton.IsEnabled = true;


                artificalBasis = await Utils.DoSimplexIteration(artificalBasis);

                if (artificalBasis != null)
                {
                    localBasis = artificalBasis.basisList;
                }
                else
                    break;
            }

            if (artificalBasis == null && Utils.ContainsArtificial(localBasis, xAmount, conditions.Count))
            {
                InputCanvas.Children.Remove(InputCanvas.Children[InputCanvas.Children.Count - 1]);

                Label labeld = new Label();
                labeld.Content = "Искуственный базис невозможно найти";
                labeld.Height = 50;
                labeld.FontSize = 20;
                labeld.Uid = "end";
                InputCanvas.Children.Add(labeld);
                InputCanvas.Height += labeld.Height;
                return;
            }


            artificalBasis.drawSimplexTable();
            InputCanvas.Children.Add(artificalBasis);
            InputCanvas.Height += artificalBasis.Canvas.Height;
            if (InputCanvas.Width <= artificalBasis.Canvas.Width)
            {
                InputCanvas.Width = artificalBasis.Canvas.Width + 25;
            }




            Label label = new Label();
            label.Content = "Искуственный базис = (" + string.Join(" ; ", Utils.makePlan(artificalBasis).ToList()) + " )";
            label.Height = 50;
            label.FontSize = 20;
            label.Uid = "end";
            InputCanvas.Children.Add(label);
            InputCanvas.Height += label.Height;
            if (!Utils.AutoSimplex)
                previousButton.IsEnabled = true;

            newMainElement = null;

            basisList = artificalBasis.basisList;
            List<LinearCondition> newConditions = conditions.ToList();
            if (basisList.Length != conditions.Count)
            {
                List<int> removed = Utils.findRemoved(artificalBasis, xAmount);
                if (removed != null)
                {
                    removed.Sort();
                    removed.Reverse();
                    for (int i = 0; i < removed.Count; i++)
                    {
                        newConditions.RemoveAt(removed[i] - (xAmount - conditions.Count));
                    }
                }
            }

            SimplexTable simplexTable = new SimplexTable(newConditions, function, basisList);

            simplexTable.drawSimplexTable();

            tables.Add(simplexTable);

            InputCanvas.Children.Add(simplexTable);
            InputCanvas.Height += simplexTable.Canvas.Height;
            InputCanvas.Width += simplexTable.Canvas.Width;

            if (Utils.CheckSimplexTable(simplexTable))
                await RunSimplex(simplexTable);



        }
        /// <summary>
        /// Функция шага назад, если вызвана с флагом withStart также запустит симплекс метод от предыдущего шага
        /// </summary>
        /// <param name="withStart">Параметр запуска с предыдущим шагом<br></br>
        /// True если запускать False если просто шаг назад </param>
        private async void PreviousStep(bool withStart = true)
        {
            int lastIndex = tables.Count - 1;
            SimplexTable toRemove = tables[lastIndex];

            int toRemoveIter = toRemove.iter;

            InputCanvas.Children.Remove(toRemove);
            tables.Remove(toRemove);
            var lastChild = InputCanvas.Children[InputCanvas.Children.Count - 1];

            if (((UIElement)lastChild).Uid == "end")
            {
                InputCanvas.Children.Remove(lastChild);
                lastChild = InputCanvas.Children[InputCanvas.Children.Count - 1];
                InputCanvas.Children.Remove(lastChild);
                InputCanvas.Height -= 100;
            }
            else
            {
                InputCanvas.Children.Remove(lastChild);
                InputCanvas.Height -= 50;
            }
            InputCanvas.Height -= toRemove.Canvas.Height;
            toRemove = null;

            SimplexTable prev = tables[lastIndex - 1];

            int prevIter = prev.iter;

            if (toRemoveIter < prev.iter)
            {
                lastChild = InputCanvas.Children[InputCanvas.Children.Count - 1];
                if (lastChild is Canvas)
                {
                    InputCanvas.Height -= ((Canvas)lastChild).Height;
                    InputCanvas.Children.Remove(lastChild);
                    tables.Remove(tables.FirstOrDefault(row => row.Canvas == lastChild));
                    lastChild = InputCanvas.Children[InputCanvas.Children.Count - 1];
                    InputCanvas.Children.Remove(lastChild);
                    InputCanvas.Height -= 50;
                }
                else
                {
                    InputCanvas.Children.Remove(lastChild);
                    InputCanvas.Height -= 50;
                    lastChild = InputCanvas.Children[InputCanvas.Children.Count - 1];
                    InputCanvas.Children.Remove(lastChild);
                    InputCanvas.Height -= 50;
                }
            }
            InputCanvas.Children.Remove(prev);
            tables.Remove(prev);
            prev = prev.Copy();
            tables.Add(prev);
            prev.drawSimplexTable();

            if (tables.Count == 1 && !Utils.AutoSimplex)
            {
                previousButton.IsEnabled = false;
            }

            InputCanvas.Children.Add(prev);
            InputCanvas.Height += prev.Canvas.Height;




            if (withStart)
                if (prev.artificialTable)
                {
                    SimplexTable prevCopy = prev.Copy();
                    if (Utils.findMainElements(prevCopy).Count == 0 && prevCopy.basisList.Length != prevCopy.conditions.Count)
                    {
                        PreviousStep();
                        return;
                    }
                    await RunSimplexWithArtificial(prev);
                }
                else
                {
                    await RunSimplex(prev);
                }

        }
        /// <summary>
        /// Событие обрабатывающее предыдущий шаг вызванный нажатием на опорный элемент
        /// </summary>
        /// <param name="newMainElement">Новый опорный элемент</param>
        /// <param name="simplexTable">Сиплекс таблица в которой произошел ивент</param>
        private void PreviousStepEvent(List<int> newMainElement, SimplexTable simplexTable)
        {
            this.newMainElement = newMainElement;
            int count = System.Math.Abs(tables.Last().iter - simplexTable.iter);
            if (tables.Last().artificialTable != simplexTable.artificialTable)
            {
                while (tables[tables.Count - 2].iter != simplexTable.iter || simplexTable.artificialTable != tables[tables.Count - 2].artificialTable)
                {
                    PreviousStep(false);
                }
                PreviousStep(true);
                newMainElement = null;
            }
            else
            {
                for (int i = 0; i != count; i++)
                {
                    PreviousStep(i == count - 1);
                }
                newMainElement = null;
            }
        }
        #endregion mainLogic



    }
}