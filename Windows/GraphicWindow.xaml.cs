using LinearTools;
using LinearTools.GraphicMethod;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Matrix = LinearTools.Matrix;


namespace LinearProgramming.Windows
{
    /// <summary>
    /// Interaction logic for GraphicWindow.xaml
    /// </summary>
    public partial class GraphicWindow : Window
    {
        MainWindow mainWindow { get; set; }
        int xAmount { get; set; }
        int conditionsAmount { get; set; }
        List<LinearCondition> conditions { get; set; }
        LinearFunction function { get; set; }
        List<int> basisList { get; set; }
        List<int> freeList { get; set; }
        double currentScale = 1;
        Graphic graphic { get; set; }
        bool fractionBool { get; set; }
        List<LinearData> datas { get; set; }
        List<FractionPoint> endPoints { get; set; }
        Matrix equalsGauss { get; set; }
        bool allAreas { get; set; }

        public GraphicWindow(MainWindow mainWindow, LinearFunction function, List<LinearCondition> conditions)
        {
            InitializeComponent();

            this.mainWindow = mainWindow;
            this.xAmount = function.xAmount;
            this.conditionsAmount = conditions.Count();

            this.function = function;
            this.conditions = conditions;

            this.SizeChanged += FullScreenScaling;
            Loaded += (object sender, RoutedEventArgs e) =>
            {
                this.WindowState = mainWindow.WindowState;

                CheckNullColumns();

                Initialize();

            };


        }
        #region UnImportant
        /// <summary>
        /// Вернуться назад
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
            Initialize();
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

                InputCanvas.Width = InputCanvas.Width * scaleX + (255 * scaleX - 250);
                InputCanvas.Height = InputCanvas.Height * scaleY + (55 * scaleY - 50);
                progressBar.Width = progressBar.Width * scaleX;
            }
            else
            {
                TaskBorder.Width = 1132;
                InputBorder.Height = 733;
                InputBorder.Width = 1132;
                InputCanvas.Height = 733;
                InputCanvas.Width = 1132;
                ActionsBorder.Height = 678;
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
        #endregion UnImportant

        #region Buttons
        /// <summary>
        /// Обработка кнопки изменения дробного режима и десятичного
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void numberSwitchButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            currentScale = graphic.Scale;
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
        /// Обработка кнопки Показать все области <br></br>
        /// Показывает область значений графика областью над ним или под ним
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ShowAllAreasButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            switch (button.Content)
            {
                case "Отобразить все области":
                    {
                        foreach (GraphicLine line in graphic.conditionLines.Where(x => x.color != Brushes.Transparent))
                        {
                            line.DrawArea();
                        }
                        allAreas = true;
                        graphic.allAreas = true;
                        button.Content = "Скрыть все области";
                        break;
                    }
                case "Скрыть все области":
                    {
                        allAreas = false;
                        foreach (GraphicLine line in graphic.conditionLines)
                            line.RemoveArea();
                        graphic.allAreas = false;
                        button.Content = "Отобразить все области";
                        break;
                    }
            }

        }
        #endregion Buttons


        public async void Initialize()
        {
            InputCanvas.Children.Clear();

            bool result = await CheckConditions();
            if (!result)
            {
                mainWindow.Show();
                this.Close();
                return;
            }

            datas = PrepareData();

            if (datas == null || datas.Count == 0)
            {
                MessageBox.Show("Не удалось преобразовать данные");
                mainWindow.Show();
                this.Close();
                return;
            }


            if (freeList.Last() == xAmount)
            {
                freeList[1] = 0;
            }

            graphic = new Graphic(InputCanvas.Height - 13, InputCanvas.Height - 13, currentScale, freeList);
            InputCanvas.Children.Add(graphic.Canvas);

            int c = 0;
            foreach (LinearData data in datas)
            {
                if (data is LinearCondition condition)
                {

                    GraphicLine line = new GraphicLine(condition, Utils.GetColorForFunction(c), 2);
                    line.DrawLine(graphic);
                    graphic.conditionLines.Add(line);
                    c++;
                }
            }

            graphic.function = (LinearFunction)datas.Last();

            try
            {
                graphic.isRestricted = await Utils.isRestricted(graphic);
            }
            catch
            {
                graphic.isRestricted = false;
            }
            GraphicLine lineX = new GraphicLine(1);
            GraphicLine lineY = new GraphicLine(2);
            graphic.conditionLines.Add(lineX);
            graphic.conditionLines.Add(lineY);
            graphic.DrawPolygram();
            endPoints = graphic.DrawMainFunction((LinearFunction)datas.Last());
            CreateInputInfo();

            if (allAreas)
            {
                foreach (GraphicLine line in graphic.conditionLines.Where(x => x.color != Brushes.Transparent))
                {
                    line.DrawArea();
                }
                graphic.allAreas = true;
            }
            else
            {
                foreach (GraphicLine line in graphic.conditionLines)
                    line.RemoveArea();
                graphic.allAreas = false;
            }
        }


        /// <summary>
        /// Функция преобразования данных<br></br>
        /// Неравенства в равенства, выражения переменных методом гаусса
        /// </summary>
        /// <returns></returns>
        public List<LinearData> PrepareData()
        {
            List<LinearData> linearDatas = new List<LinearData>();
            List<LinearCondition> equals = conditions.Where(x => x.conditionType == ConditionType.Equal).ToList();
            List<LinearCondition> nonEquals = conditions.Where(x => x.conditionType != ConditionType.Equal).ToList();
            List<LinearCondition> newEquals = new List<LinearCondition>();
            List<LinearCondition> newNonEquals = new List<LinearCondition>();
            freeList = new List<int> { 0, 1 };
            if (xAmount == 2)
            {
                foreach (LinearCondition condition in equals)
                {
                    linearDatas.Add(condition);
                }
                foreach (LinearCondition condition in nonEquals)
                {
                    linearDatas.Add(condition);
                }
                linearDatas.Add(function);
                return linearDatas;
            }
            if (xAmount - equals.Count > 2)
            {
                return linearDatas;
            }
            if (xAmount == 1)
            {
                foreach (LinearCondition condition in equals)
                {
                    List<Fraction> data = condition.getData();
                    Fraction last = data.Last();
                    data.RemoveAt(1);
                    data.Add(new Fraction(0));
                    data.Add(last);
                    LinearCondition cnd = new LinearCondition(2, data);
                    linearDatas.Add(cnd);
                }
                foreach (LinearCondition condition in nonEquals)
                {
                    List<Fraction> data = condition.getData();
                    Fraction last = data.Last();
                    data.RemoveAt(1);
                    data.Add(new Fraction(0));
                    data.Add(last);
                    LinearCondition cnd = new LinearCondition(2, data, condition.conditionType);
                    linearDatas.Add(cnd);
                }
                List<Fraction> fData = function.GetRawData();
                Fraction lData = fData.Last();
                fData.RemoveAt(1);
                fData.Add(new Fraction(0));
                fData.Add(lData);
                LinearFunction fnc = new LinearFunction(2, fData, function.min);
                linearDatas.Add(fnc);
                return linearDatas;
            }
            basisList.Sort();
            freeList = Enumerable.Range(0, xAmount).Except(basisList).ToList();

            equalsGauss = Utils.doGauss(new Matrix(equals), basisList);
            if (!Utils.isLineryIndependent(equalsGauss, basisList))
            {
                return datas;
            }

            if (xAmount == basisList.Count)
            {
                freeList = new List<int> { 0, 1 };
                for (int i = 0; i != 2; i++)
                {
                    LinearCondition condition = new LinearCondition(2, new List<Fraction> { new Fraction(1 - i), new Fraction(i), equalsGauss.Conditions[i].Last() });
                    newEquals.Add(condition);
                }
                for (int i = 0; i != nonEquals.Count; i++)
                {
                    for (int k = 2; k != equals.Count; k++)
                    {
                        List<Fraction> nonEqualData = nonEquals[i].getData();
                        int j = equalsGauss.Conditions[k].Count - 2;
                        Fraction scale = nonEqualData[j];
                        nonEqualData[j] -= scale * equalsGauss.Conditions[k][j];
                        nonEqualData[nonEqualData.Count - 1] -= scale * equalsGauss.Conditions[k][j + 1];

                        nonEqualData = nonEqualData.Take(2).Concat(new[] { nonEqualData.Last() }).ToList();
                        LinearCondition condition = new LinearCondition(2, nonEqualData, nonEquals[i].conditionType);
                        newNonEquals.Add(condition);
                    }
                }
                foreach (LinearCondition cnd in newEquals.Union(newNonEquals))
                {
                    linearDatas.Add(cnd);
                }
                List<Fraction> functionData = function.GetRawData();
                for (int i = 2; i != equals.Count; i++)
                {
                    Fraction scale = functionData[i];
                    functionData[i] -= scale * equalsGauss.Conditions[i][i];
                    functionData[functionData.Count - 1] -= scale * equalsGauss.Conditions[i].Last();
                }
                functionData = functionData.Take(2).Concat(new[] { functionData.Last() }).ToList();
                LinearFunction newFunction = new LinearFunction(2, functionData, function.min);
                linearDatas.Add(newFunction);
            }
            else
            {
                if (freeList.Count == 1)
                {
                    freeList.Add(xAmount);
                }
                for (int i = 0; i != equals.Count; i++)
                {
                    List<Fraction> conditionData = equalsGauss.Conditions[i];
                    conditionData = freeList
                    .Select(index => index == equalsGauss.Conditions[i].Count - 1
                        ? new Fraction(0)
                        : equalsGauss.Conditions[i][index])
                    .Concat(new[] { equalsGauss.Conditions[i].Last() })
                    .ToList();
                    LinearCondition condition = new LinearCondition(2, conditionData, ConditionType.LessOrEqualThen);
                    newEquals.Add(condition);
                }
                for (int i = 0; i != nonEquals.Count; i++)
                {
                    List<Fraction> conditionData = nonEquals[i].getData();
                    for (int j = 0; j != equalsGauss.Conditions.Count; j++)
                    {
                        List<Fraction> equalData = equalsGauss.Conditions[j];
                        if (conditionData[basisList[j]] != 0)
                        {
                            Fraction scale = conditionData[basisList[j]];
                            for (int k = 0; k != equalData.Count; k++)
                            {
                                conditionData[k] -= scale * equalData[k];
                            }
                        }
                    }
                    conditionData = freeList
                    .Select(index => index == conditionData.Count - 1
                        ? new Fraction(0)
                        : conditionData[index])
                    .Concat(new[] { conditionData.Last() })
                    .ToList();
                    LinearCondition condition = new LinearCondition(2, conditionData, nonEquals[i].conditionType);
                    newNonEquals.Add(condition);
                }
                List<Fraction> functionData = function.GetRawData();
                for (int i = 0; i != equalsGauss.Conditions.Count; i++)
                {
                    List<Fraction> equalData = equalsGauss.Conditions[i];
                    if (functionData[basisList[i]] != 0)
                    {
                        Fraction scale = functionData[basisList[i]];
                        for (int k = 0; k != equalData.Count - 1; k++)
                        {
                            functionData[k] -= scale * equalData[k];
                        }

                        functionData[equalData.Count - 1] += scale * equalData[equalData.Count - 1]; ///12312
                    }
                }
                functionData = freeList
                    .Select(index => index == functionData.Count - 1
                    ? new Fraction(0)
                    : functionData[index])
                    .Concat(new[] { functionData.Last() })
                    .ToList();
                LinearFunction newFunction = new LinearFunction(2, functionData, function.min);

                foreach (LinearCondition condition in newEquals.Union(newNonEquals))
                {
                    linearDatas.Add(condition);
                }
                linearDatas.Add(newFunction);
            }


            return linearDatas;
        }

        /// <summary>
        /// Отрисовка ответа задачи
        /// </summary>
        public void CreateInputInfo()
        {
            var elementsWithUidBorder = InputCanvas.Children
            .OfType<FrameworkElement>()
            .Where(child => child.Uid == "InputBorder").FirstOrDefault();
            InputCanvas.Children.Remove(elementsWithUidBorder);

            ScrollViewer inputScroll = new ScrollViewer();
            inputScroll.Height = InputCanvas.Height - 13;
            inputScroll.Width = InputCanvas.Width - InputCanvas.Height - 7;
            inputScroll.VerticalAlignment = VerticalAlignment.Top;
            inputScroll.HorizontalAlignment = HorizontalAlignment.Left;
            inputScroll.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;
            inputScroll.HorizontalScrollBarVisibility = ScrollBarVisibility.Visible;

            Border border = new Border();
            border.Height = inputScroll.Height;
            border.Width = inputScroll.Width;
            border.BorderBrush = Brushes.Black;
            border.BorderThickness = new Thickness(2);
            border.Child = inputScroll;
            border.Uid = "InputBorder";

            InputCanvas.Children.Add(border);
            Canvas.SetRight(border, 15);

            Canvas inputPanel = new Canvas();
            inputPanel.Height = inputScroll.Height;
            inputPanel.Width = inputScroll.Width-25;
            inputScroll.Content = inputPanel;
            inputPanel.HorizontalAlignment = HorizontalAlignment.Left;

            Label task = new Label();
            task.Width = inputPanel.Width;
            task.Height = 50;
            task.Content = "Задача";
            task.VerticalContentAlignment = VerticalAlignment.Center;
            task.HorizontalContentAlignment = HorizontalAlignment.Center;
            task.FontSize = 23;
            task.FontWeight = FontWeights.Bold;

            inputPanel.Children.Add(task);
            Canvas.SetLeft(task, 5);
            Canvas.SetTop(task, 5);

            LinearFunction mainF = (LinearFunction)datas.Last();
            List<Fraction> mainFList = mainF.GetRawData();
            Label mainFunction = new Label();
            mainFunction.Height = 50;
            mainFunction.Content = "F = " + mainFList[0].ToString() + "x" + Utils.makeLowerIndex(freeList[0] + 1);

            if (freeList[1] != 0)
            {
                mainFunction.Content += " " + (mainFList[1] >= 0 ? "+" : "") + " " + mainFList[1] + "x" + Utils.makeLowerIndex(freeList[1] + 1);
            }

            mainFunction.Content += (mainFList[2] >= 0 ? "+" : "") + " " + mainFList[2];


            mainFunction.Content += " -> ";
            if (mainF.min)
                mainFunction.Content += " min";
            else
                mainFunction.Content += " max";
            mainFunction.FontSize = 20;
            mainFunction.VerticalContentAlignment = VerticalAlignment.Center;
            mainFunction.HorizontalContentAlignment = HorizontalAlignment.Left;

            

            Rectangle colorSquare = new Rectangle
            {
                Width = 20,
                Height = 20,
                Fill = Brushes.Red
            };
            Border borderedSquare = new Border
            {
                BorderThickness = new Thickness(1),
                BorderBrush = Brushes.Black,
                Child = colorSquare,
            };

            Canvas.SetLeft(borderedSquare, 5);
            Canvas.SetTop(borderedSquare, 69);

            inputPanel.Children.Add(borderedSquare);

            Canvas.SetLeft(mainFunction, 30);
            Canvas.SetTop(mainFunction, 55);

            inputPanel.Children.Add(mainFunction);

            Label cndtions = new Label();
            cndtions.Height = 50;
            cndtions.Width = inputPanel.Width;
            cndtions.Content = "Ограничения";
            cndtions.VerticalContentAlignment = VerticalAlignment.Center;
            cndtions.HorizontalContentAlignment = HorizontalAlignment.Center;
            cndtions.FontSize = 20;
            cndtions.FontWeight = FontWeights.Bold;

            inputPanel.Children.Add(cndtions);
            Canvas.SetLeft(cndtions, 5);
            Canvas.SetTop(cndtions, 105);

            int count = 0;
            for (int i = 0; i != datas.Count - 1; i++)
            {

                LinearCondition cnd = (LinearCondition)datas[i];
                List<Fraction> fractionData = cnd.getData();
                if (fractionData[0] == 0 && fractionData[1] == 0)
                    continue;
                Label condition = new Label();
                condition.Height = 50;
                condition.HorizontalContentAlignment = HorizontalAlignment.Left;
                condition.VerticalContentAlignment = VerticalAlignment.Center;
                condition.Content = fractionData[0].ToString() + "x" + Utils.makeLowerIndex(freeList[0] + 1);
                if (freeList[1] != 0)
                {
                    condition.Content += " " + (fractionData[1] >= 0 ? "+" : "") + " " + fractionData[1] + "x" + Utils.makeLowerIndex(freeList[1] + 1);
                }
                condition.Content += cnd.conditionType.GetOperator() + " " + fractionData[2];
                condition.FontSize = 18;
                inputPanel.Children.Add(condition);
                Canvas.SetLeft(condition, 30);
                Canvas.SetTop(condition, 160 + 50 * count);


                Rectangle conditionSquare = new Rectangle
                {
                    Width = 20,
                    Height = 20,
                    Fill = Utils.GetColorForFunction(i)
                };
                conditionSquare.MouseDown += (sender, e) =>
                {
                    GraphicLine line = graphic.conditionLines.Where(x => x.color == conditionSquare.Fill).First();
                    if (line.area == null)
                        line.DrawArea();
                    else
                        line.RemoveArea();
                };
                Border borderedConditionSquare = new Border
                {
                    BorderThickness = new Thickness(1),
                    BorderBrush = Brushes.Black,
                    Child = conditionSquare,
                };

                inputPanel.Children.Add(borderedConditionSquare);

                Canvas.SetLeft(borderedConditionSquare, 5);
                Canvas.SetTop(borderedConditionSquare, 172 + 50 * count);
                count++;

            }
            Label answer = new Label();
            answer.Height = 50;
            answer.Width = inputPanel.Width;
            answer.Content = "Ответ";
            answer.VerticalContentAlignment = VerticalAlignment.Center;
            answer.HorizontalContentAlignment = HorizontalAlignment.Center;
            answer.FontSize = 20;
            answer.FontWeight = FontWeights.Bold;

            inputPanel.Children.Add(answer);
            Canvas.SetLeft(answer, 5);
            Canvas.SetTop(answer, 177 + 50 * count);

            mainFunction.Loaded += (sender, e) =>
            {
                if (mainFunction.ActualWidth > inputPanel.Width)
                {
                    inputPanel.Width = mainFunction.ActualWidth+30;

                }
            };

            if (!graphic.isRestricted)
            {
                Label wrong = new Label();
                wrong.Height = 50;
                wrong.Width = inputPanel.Width;
                wrong.Content = "Задача неразрешима";
                wrong.VerticalContentAlignment = VerticalAlignment.Center;
                wrong.HorizontalContentAlignment = HorizontalAlignment.Center;
                wrong.FontSize = 18;

                inputPanel.Children.Add(wrong);

                Canvas.SetLeft(wrong, 5);
                Canvas.SetTop(wrong, 227 + 50 * count);

                inputPanel.Height = 0;
                foreach(FrameworkElement child in inputPanel.Children)
                {
                    inputPanel.Height += child.Height;
                }

                return;
            }
            Label answerDot = new Label();
            answerDot.Height = 50;
            answerDot.VerticalContentAlignment = VerticalAlignment.Center;
            answerDot.HorizontalContentAlignment = HorizontalAlignment.Left;
            answerDot.FontSize = 18;

            List<Fraction> answers = new List<Fraction>();

            for (int i = 0; i != endPoints.Count; i++)
            {
                Fraction f = mainFList[0] * (endPoints[i].X) + mainFList[1] * (endPoints[i].Y) + mainFList[2];
                answers.Add(f);
            }


            List<List<Fraction>> solutions = GetSolution();
            if (Fraction.decimalFlag)
            {
                Fraction ProcessFraction(Fraction fraction)
                {
                    double value = fraction.Value();
                    return Math.Abs(Math.Round(value) - value) <= 1e-9
                        ? new Fraction(Math.Round(value))
                        : new Fraction(Math.Round(value, 10));
                }

                for (int i = 0; i < solutions.Count; i++)
                {
                    solutions[i] = solutions[i]
                        .Select(ProcessFraction)
                        .ToList();
                }

                answers = answers
                    .Select(ProcessFraction)
                    .ToList();
            }

            answerDot.Content = "F = " + answers[0].ToString();
            answerDot.Content += " в точке (";
            for (int i = 0; i != solutions[0].Count; i++)
            {
                answerDot.Content += solutions[0][i].ToString() + "";
                if (i != solutions[0].Count - 1)
                    answerDot.Content += "; ";
            }
            answerDot.Content += ")";
            if (answers.Count > 1)
            {
                answerDot.Height = 100;
                answerDot.Content += "\n";
                answerDot.Content += "F = " + answers[1].ToString();
                answerDot.Content += " в точке (";
                for (int i = 0; i != solutions[1].Count; i++)
                {
                    answerDot.Content += solutions[1][i].ToString() + "";
                    if (i != solutions[1].Count - 1)
                        answerDot.Content += "; ";
                }
                answerDot.Content += ")";
            }

            inputPanel.Children.Add(answerDot);
            Canvas.SetLeft(answerDot, 5);
            Canvas.SetTop(answerDot, 227 + 50 * count);

            inputPanel.VerticalAlignment = VerticalAlignment.Top;

            answerDot.Loaded += (sender, e) =>
            {
                if (answerDot.ActualWidth > inputPanel.Width)
                {
                    inputPanel.Width = answerDot.ActualWidth+30;

                }
                inputPanel.Height = 0;
                foreach (FrameworkElement child in inputPanel.Children)
                {
                    inputPanel.Height += child.Height;
                }
            };

        }

        /// <summary>
        /// Функция получения точки ответа
        /// </summary>
        /// <returns></returns>
        public List<List<Fraction>> GetSolution()
        {
            List<List<Fraction>> solutions = new List<List<Fraction>>();

            if (basisList == null)
            {
                for (int i = 0; i != endPoints.Count; i++)
                {
                    List<Fraction> solution = new List<Fraction>();
                    solution.Add(endPoints[i].X);
                    solution.Add(endPoints[i].Y);
                    solutions.Add(solution);
                }
                return solutions;
            }
            else
            {
                for (int j = 0; j != endPoints.Count; j++)
                {
                    FractionPoint endPoint = endPoints[j];

                    List<Fraction> solution = new List<Fraction>();
                    Fraction[] point = new Fraction[xAmount];
                    if (freeList[1] != 0)
                        point[freeList[1]] = endPoint.Y;
                    point[freeList[0]] = endPoint.X;

                    for (int i = 0; i != basisList.Count; i++)
                    {
                        Fraction pnt = equalsGauss.Conditions[i][equalsGauss.Conditions[i].Count - 1] - equalsGauss.Conditions[i][freeList[0]] * endPoint.X;
                        if (freeList[1] != 0)
                            pnt -= equalsGauss.Conditions[i][freeList[1]] * endPoint.Y;
                        point[basisList[i]] = pnt;
                    }
                    for (int i = 0; i != xAmount; i++)
                    {
                        solution.Add(point[i]);
                    }
                    solution = solution.Select(x => { if (Math.Abs(x.Value()) < 1e-9) x = new Fraction(0); return x; }).ToList();
                    solutions.Add(solution);
                }
                return solutions;
            }
        }

        /// <summary>
        /// Проверка условий на совместность, линейную зависимость и решимость задачи графическим методом
        /// </summary>
        public async Task<bool> CheckConditions()
        {
            if (conditions.Count(x => x.conditionType == ConditionType.Equal) != 0 && xAmount > 2)
            {
                List<LinearCondition> newConditions = await Utils.RemoveDependentLines(conditions.Where(x => x.conditionType == ConditionType.Equal).ToList());
                if (newConditions == null)
                {
                    MessageBox.Show("Линейные ограничение несовместны");
                    return false;
                }
                else if (newConditions.Count != conditions.Count(x => x.conditionType == ConditionType.Equal))
                {
                    MessageBox.Show(string.Format("Удалено {0} линейно-зависимых строк", conditions.Count(x => x.conditionType == ConditionType.Equal) - newConditions.Count));
                    conditions = conditions.Where(x => x.conditionType != ConditionType.Equal).ToList();
                    conditions = conditions.Union(newConditions).ToList();
                }

                if (basisList == null)
                {
                    BasisSelection basisSelection = new BasisSelection(conditions[0].xAmount, conditions.Count(x => x.conditionType == ConditionType.Equal));
                    if (basisSelection.ShowDialog() == true)
                    {
                        basisList = basisSelection.selectedX.ToList();
                        basisList.Sort();
                    }
                    else
                    {
                        SimplexTable artifical = await Utils.AutoFindBasisWithSimplex(conditions.Where(x => x.conditionType == ConditionType.Equal).ToList());
                        if (artifical == null)
                        {
                            {
                                if (xAmount < conditions.Count(x => x.conditionType == ConditionType.Equal))
                                {
                                    MessageBox.Show("Невозможно построить график данной задачи");
                                    return false;
                                }

                                basisList = await Utils.AutoFindBasisAsync(new Matrix(conditions.Where(x => x.conditionType == ConditionType.Equal).ToList()), null);
                                if (basisList.Count == 0)
                                {
                                    MessageBox.Show("Невозможно построить график данной задачи");
                                    return false;

                                }
                            }

                        }
                        else
                        {
                            basisList = artifical.basisList.ToList();
                        }
                    }
                }
            }
            else if (xAmount > 2)
            {
                MessageBox.Show("Невозможно построить график данной задачи");
                return false;
            }
            return true;
        }

        /// <summary>
        /// Проверка нулевых столбцов в условиях задачи<br></br>
        /// И удаление их если они присутствуют
        /// </summary>
        private void CheckNullColumns()
        {
            List<int> columnList = new List<int>();
            List<Fraction> functionData = function.GetRawData();
            for (int i = 0; i != xAmount; i++)
            {
                if (!functionData[i].isEqualTo(new Fraction(0)))
                    continue;
                bool breaked = false;
                foreach (LinearCondition cnd in conditions)
                {
                    List<Fraction> cndData = cnd.getData();
                    if (!cndData[i].isEqualTo(new Fraction(0)))
                    {
                        breaked = true;
                        break;
                    }
                }
                if (!breaked)
                    columnList.Add(i);
            }
            if (columnList.Count != 0)
            {
                MessageBox.Show("Удалено " + columnList.Count + " нулевых столбцов");
                this.xAmount = xAmount - columnList.Count;

                List<Fraction> functData = function.GetRawData();
                for (int i = 0; i != columnList.Count; i++)
                {
                    functData.RemoveAt(columnList[i]);
                }
                this.function = new LinearFunction(xAmount, functData, function.min);

                List<LinearCondition> newConditions = new List<LinearCondition>();
                for (int i = 0; i != conditions.Count; i++)
                {
                    List<Fraction> condData = conditions[i].getData();
                    for (int j = 0; j != columnList.Count; j++)
                    {
                        condData.RemoveAt(columnList[j]);
                    }
                    LinearCondition condition = new LinearCondition(xAmount, condData, conditions[i].conditionType);
                    newConditions.Add(condition);
                }
                this.conditions = newConditions;
                this.conditionsAmount = conditions.Count();
            }
        }
    }
}
