using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace LinearTools
{
    public static class Utils
    {
        /// <summary>
        /// Поле для управления автоматизацией симплекс метода<br></br>
        /// True - автоматический, False - ручной
        /// </summary>
        public static bool AutoSimplex = true;
        /// <summary>
        /// Событие для отслеживания операций симплекс метода<br></br>
        /// 
        /// </summary>
        public static event Action<string, List<Fraction>> SimplexOperation;
        /// <summary>
        /// Событие чтобы отслеживать изменения в методе гаусса<br></br>
        /// Распространяется на вычитание и деление строк
        /// </summary>
        public static event Action<Matrix, string, List<Fraction>> GaussOperation;
        /// <summary>
        /// События отслеживания шага назад если он сделан на ранних симплекс таблицах
        /// </summary>
        public static event Action<List<int>, SimplexTable> PreviousStep;
        /// <summary>
        /// Функция отписки от всех функций события SimplexOperation
        /// </summary>
        public static void ResetSimplexOperation()
        {
            SimplexOperation = delegate { };
            PreviousStep = delegate { };
        }
        /// <summary>
        /// Функция отписки от всех функций события GaussOperation
        /// </summary>
        public static void ResetGaussOperation() => GaussOperation = delegate { };

        private static readonly SolidColorBrush[] colors = new SolidColorBrush[]
{
    Brushes.Yellow, // Желтый
    Brushes.Green,  // Зеленый
    Brushes.Indigo, // Индиго
    Brushes.Violet, // Фиолетовый
    Brushes.Navy,       // Темно-синий
    Brushes.Cyan,       // Бирюзовый
    Brushes.Teal,       // Бирюзово-зеленый
    Brushes.DarkGreen,  // Темно-зеленый
    Brushes.Maroon,     // Бордовый
    Brushes.HotPink,    // Ярко-розовый
};



        #region UnImportant
        /// <summary>
        /// Возвращает нижний индекс
        /// </summary>
        /// <param name="number">Число превращаемое в нижний индекс</param>
        /// <returns></returns>
        public static string makeLowerIndex(int number)
        {
            string[] subscriptNumbers =
             {
                "\u2080", "\u2081", "\u2082", "\u2083", "\u2084",
                "\u2085", "\u2086", "\u2087", "\u2088", "\u2089"
             };
            string subscript = "";
            if (number < 10)
            {
                subscript = subscriptNumbers[number];
            }
            else
            {
                foreach (char digit in number.ToString())
                {
                    int digitValue = digit - '0';
                    subscript += subscriptNumbers[digitValue];
                }
            }
            return subscript;
        }
        /// <summary>
        /// Возвращает верхний индекс
        /// </summary>
        /// <param name="number">Число преврещаемое в верхний индекс</param>
        /// <returns></returns>
        public static string makeUpperIndex(int number)
        {
            string[] superscriptNumbers = {
                "\u2070", "\u00B9", "\u00B2", "\u00B3", "\u2074",
                "\u2075", "\u2076", "\u2077", "\u2078", "\u2079"
            };
            string subscript = "";
            if (number < 10)
            {
                subscript = superscriptNumbers[number];
            }
            else
            {
                foreach (char digit in number.ToString())
                {
                    int digitValue = digit - '0';
                    subscript += superscriptNumbers[digitValue];
                }
            }
            return subscript;
        }

        public static SolidColorBrush GetColorForFunction(int index)
        {
            return colors[index];
        }


        public static Fraction ConvertToFraction(double value, int maxDenominator = 1000)
        {
            int sign = value < 0 ? -1 : 1; // Учитываем знак числа
            value = Math.Abs(value);

            // Если число целое, возвращаем дробь с знаменателем 1
            if (value % 1 == 0)
                return new Fraction((int)value * sign, 1);

            // Инициализация переменных
            int numerator = 0, denominator = 1, prevNumerator = 1, prevDenominator = 0;
            double fraction = value;

            // Используем алгоритм продолженных дробей
            while (true)
            {
                int integerPart = (int)Math.Floor(fraction);
                int tempNumerator = numerator, tempDenominator = denominator;

                numerator = integerPart * numerator + prevNumerator;
                denominator = integerPart * denominator + prevDenominator;

                prevNumerator = tempNumerator;
                prevDenominator = tempDenominator;

                // Проверяем ограничение на знаменатель
                if (denominator > maxDenominator)
                    break;

                // Проверяем точность
                if (Math.Abs((double)numerator / denominator - value) < 1e-9)
                    break;

                // Избегаем деления на ноль
                if (fraction - integerPart == 0)
                    break;

                fraction = 1 / (fraction - integerPart);
            }

            Fraction newFraction = new Fraction(numerator * sign, sign * denominator);
            newFraction.decimalValue = value;
            return newFraction;
        }


        #endregion UnImportant

        #region gauss
        /// <summary>
        /// Асинхронный метод гаусса 
        /// </summary>
        /// <param name="matrix">Список списков Fraction</param>
        /// <param name="basis">Базисные переменные</param>
        /// <returns>Матрицу приведенную к ступенчатому виду</returns>
        public static async Task<Matrix> doGaussAsync(Matrix matrix, List<int> basis, IProgress<int> progress = null)
        {
            int row = matrix.Row;
            int column = matrix.Column;
            Matrix output = matrix.Copy();
            int totalOperations = basis.Count * (basis.Count - 1) + basis.Count; // Оценка общего числа операций для прогресса
            int currentOperation = 0;

            // Первая фаза: приведение к нулю элементов ниже диагонали
            for (int i = 0; i < basis.Count - 1; i++)
            {
                for (int j = i + 1; j < basis.Count; j++)
                {
                    Fraction scale = findScaleToNull(j, i, i, output, basis);
                    if (scale != null)
                    {
                        minus(j, i, scale, output);
                        GaussOperation?.Invoke(output, "minus", new List<Fraction> { new Fraction(i + 1), new Fraction(j + 1), scale });
                    }

                    // Обновление прогресса
                    currentOperation++;
                    progress?.Report((currentOperation * 100) / totalOperations);
                    await Task.Delay(1);
                }
            }

            // Вторая фаза: приведение к нулю элементов выше диагонали
            for (int i = basis.Count - 1; i > 0; i--)
            {
                for (int j = i - 1; j >= 0; j--)
                {
                    Fraction scale = findScaleToNull(j, i, i, output, basis);
                    if (scale != null)
                    {
                        minus(j, i, scale, output);
                        GaussOperation?.Invoke(output, "minus", new List<Fraction> { new Fraction(i + 1), new Fraction(j + 1), scale });
                    }

                    // Обновление прогресса
                    currentOperation++;
                    progress?.Report((currentOperation * 100) / totalOperations);
                    await Task.Delay(1);
                }
            }

            // Третья фаза: нормализация диагональных элементов
            for (int i = 0; i < basis.Count; i++)
            {
                if (output.Conditions[i][basis[i]] != 1 && output.Conditions[i][basis[i]] != 0)
                {
                    Fraction koeff = output.Conditions[i][basis[i]];
                    for (int k = 0; k < output.Column + 1; k++)
                    {
                        output.Conditions[i][k] /= koeff;
                    }
                    GaussOperation?.Invoke(output, "divide", new List<Fraction> { new Fraction(i + 1), new Fraction(basis[i] + 1), koeff });
                }

                // Обновление прогресса
                currentOperation++;
                progress?.Report((currentOperation * 100) / totalOperations);
                await Task.Delay(1);
            }

            return output;
        }
        /// <summary>
        /// Метод гаусса
        /// </summary>
        /// <param name="matrix">Матрицу</param>
        /// <param name="basis">Базисные переменные</param>
        /// <returns></returns>
        public static Matrix doGauss(Matrix matrix, List<int> basis)
        {
            int row = matrix.Row;
            int column = matrix.Column;
            Matrix output = matrix.Copy();
            for (int i = 0; i < basis.Count - 1; i++)
            {
                for (int j = i + 1; j != basis.Count; j++)
                {

                    Fraction scale = findScaleToNull(j, i, i, output, basis);
                    if (scale != null)
                    {
                        minus(j, i, scale, output);

                    }
                }
            }
            for (int i = basis.Count - 1; i != 0; i--)
            {
                for (int j = i - 1; j != -1; j--)
                {
                    Fraction scale = findScaleToNull(j, i, i, output, basis);
                    if (scale != null)
                    {
                        minus(j, i, scale, output);

                    }
                }

            }
            for (int i = 0; i < basis.Count; i++)
            {

                if (output.Conditions[i][basis[i]] != 1 && output.Conditions[i][basis[i]] != 0)
                {
                    Fraction koeff = output.Conditions[i][basis[i]];
                    for (int k = 0; k != output.Column + 1; k++)
                    {
                        output.Conditions[i][k] /= koeff;
                    }
                }
            }

            return output;
        }
        /// <summary>
        /// Операция вычитания строки в матрице
        /// </summary>
        /// <param name="firstSubRow">Уменьшаемая строка</param>
        /// <param name="secondSubRow">Вычитаемая строка</param>
        /// <param name="scale">Коэффициент на который надо умножить вычитаемое чтобы получить 0</param>
        /// <param name="output">Матрица над которой проводится операция</param>
        private static void minus(int firstSubRow, int secondSubRow, Fraction scale, Matrix output)
        {

            for (int i = 0; i <= output.Column; i++)
            {
                output.Conditions[firstSubRow][i] -= output.Conditions[secondSubRow][i] * scale;
            }
        }
        /// <summary>
        /// Нахождение коэффициента 
        /// </summary>
        /// <param name="firstSubRow">Уменьшаемая строка</param>
        /// <param name="secondSubRow">Вычитаемая строка</param>
        /// <param name="iteration">Номер итерации, чтобы найти столбец</param>
        /// <param name="output">Матрица над которой проводится операция</param>
        /// <param name="basis">Список базисных переменных</param>
        /// <returns>Возвращает коэффициент на который надо умножить вторую строку чтобы вычесть из первой и получить 0</returns>
        private static Fraction findScaleToNull(int firstSubRow, int secondSubRow, int iteration, Matrix output, List<int> basis)
        {
            int toNull = basis[iteration];




            if (output.Conditions[secondSubRow][toNull] == 0)
            {
                bool confirm = findMainRow(secondSubRow, toNull, output);
                if (confirm)
                {
                    Fraction scale = findScaleToNull(firstSubRow, secondSubRow, iteration, output, basis);
                    return scale;
                }
                else
                {
                    return null;
                }
            }


            return new Fraction(output.Conditions[firstSubRow][toNull] / output.Conditions[secondSubRow][toNull]);

        }
        /// <summary>
        /// Нахождение строчки чтобы в <paramref name="row"/> и <paramref name="column"/> не было 0 <br></br>
        /// Меняет изначальную строку на новую в матрице
        /// </summary>
        /// <param name="row">Номер строки</param>
        /// <param name="column">Номер столбца</param>
        /// <param name="output">Матрица в которой проводится операция</param>
        /// <returns>Возвращает True если успешно переставило строку и False если не нашлось такой строки</returns>
        private static bool findMainRow(int row, int column, Matrix output)
        {

            for (int i = row + 1; i < output.Conditions.Count; i++)
            {

                if (output.Conditions[i][column] != 0)
                {
                    // Переставляем строки
                    List<Fraction> temp = output.Conditions[row];
                    output.Conditions[row] = output.Conditions[i];
                    output.Conditions[i] = temp;
                    GaussOperation?.Invoke(output, "swap", new List<Fraction> { new Fraction(row + 1), new Fraction(i + 1) });
                    return true;
                }
            }
            // Если ни одна строка не подошла, возвращаем false
            return false;
        }

        /// <summary>
        /// Проверяет матрицу на совместность
        /// </summary>
        /// <param name="matrix"></param>
        /// <returns>True если матрица совместна, False если нет</returns>
        public static bool isRight(Matrix matrix)
        {
            {
                for (int i = 0; i < matrix.Row; i++)
                {
                    bool allZeroes = true;
                    for (int j = 0; j < matrix.Column; j++)
                    {
                        if (matrix.Conditions[i][j] != 0)
                        {
                            allZeroes = false;
                            break;
                        }
                    }
                    if (allZeroes && matrix.Conditions[i][matrix.Column - 1] != 0)
                    {
                        return true;
                    }
                }
                return false;
            }

        }
        /// <summary>
        /// Проверяет матрицу на линейную независимость
        /// </summary>
        /// <param name="matrix">Матрица приведенная к ступенчатому виду методом doGauss</param>
        /// <param name="basis">Базис по которому приводили</param>
        /// <returns>True если независима, False если есть линейнозависимые строки</returns>
        public static bool isLineryIndependent(Matrix matrix, List<int> basis)
        {
            for (int i = 0; i < basis.Count; i++)
            {

                if (matrix.Conditions[i][basis[i]] != 1)
                {
                    return false;
                }
            }
            return true;
        }
        /// <summary>
        /// Генерирует список базисов
        /// </summary>
        /// <param name="xAmount">Количество переменных</param>
        /// <param name="conditionsCount">Количество условий</param>
        /// <returns>Список базисов</returns>
        public static async Task<List<List<int>>> GenerateBasis(int xAmount, int conditionsCount, IProgress<int> progress)
        {
            var result = new List<List<int>>();
            var combination = Enumerable.Range(0, conditionsCount).ToList();
            int totalCombinations = 0;

            while (true)
            {
                totalCombinations++;
                int i = conditionsCount - 1;
                while (i >= 0 && combination[i] == xAmount - conditionsCount + i) i--;
                if (i < 0) break;
                combination[i]++;
                for (int j = i + 1; j < conditionsCount; j++)
                {
                    combination[j] = combination[j - 1] + 1;
                }
            }

            combination = Enumerable.Range(0, conditionsCount).ToList();
            int currentProgress = 0;

            while (true)
            {
                // Добавляем комбинацию в результат без учета перестановок
                result.Add(new List<int>(combination));
                currentProgress++;

                // Обновляем прогресс для генерации комбинаций (0% - 50%)
                int generationProgress = (currentProgress * 50) / totalCombinations;
                progress?.Report(generationProgress);

                await Task.Delay(1);

                // Поиск позиции, которую можно увеличить
                int i = conditionsCount - 1;
                while (i >= 0 && combination[i] == xAmount - conditionsCount + i) i--;

                // Если все позиции достигли максимума, завершаем
                if (i < 0) break;

                // Увеличиваем текущую позицию и сбрасываем все последующие
                combination[i]++;
                for (int j = i + 1; j < conditionsCount; j++)
                {
                    combination[j] = combination[j - 1] + 1;
                }
            }

            return result;
        }
        /// <summary>
        /// Автоматически находит базис 
        /// </summary>
        /// <param name="matrix">Матрица в которой нужно найти базис</param>
        /// <param name="xAmount">Количество переменных</param>
        /// <param name="conditionsCount">Количество условий</param>
        /// <returns>Подходящий базис либо пустой список если базис не удалось найти</returns>
        public static async Task<List<int>> AutoFindBasisAsync(Matrix matrix, IProgress<int> progress)
        {
            int xAmount = matrix.Column;
            int conditionsCount = matrix.Row;

            // Генерация базисов с прогрессом (0% - 50%)
            List<List<int>> basisList = await GenerateBasis(xAmount, conditionsCount, progress);

            // Проверка каждого базиса (50% - 100%)
            for (int i = 0; i < basisList.Count; i++)
            {
                // Обновляем прогресс выполнения основной работы
                int checkProgress = 50 + (i + 1) * 50 / basisList.Count;
                progress?.Report(checkProgress);

                await Task.Delay(1);
                LinearTools.Matrix tempMatrix = await Task.Run(() => Utils.doGauss(matrix, basisList[i]));
                if (Utils.isLineryIndependent(tempMatrix, basisList[i]))
                {
                    progress?.Report(100); // Завершаем на 100%, если нашли базис
                    await Task.Delay(1);
                    return basisList[i];
                }
            }

            progress?.Report(100); // Обновляем на 100%, если базис не найден
            return new List<int>();
        }
        #endregion gauss

        #region SimplexMethod
        /// <summary>
        /// Выделение элементов в симплекс таблице <br></br>
        /// а также привязывает к симплекс таблице события нажатия на опорный элемент
        /// </summary>
        /// <param name="simplexTable">Симплекс таблица в которой нужно совершить выделение</param>
        /// <param name="mainElements">Список опорных элементов в формате List int Row,Column </param>
        /// <param name="onlyHighlight">Параметр только выделения цветом без добавления событий </param>
        /// <param name="newMainElement">Параметр уже заданного выбранного опорного элемента</param>
        /// <returns></returns>
        private static async Task<List<int>> HighlightMainElements(SimplexTable simplexTable, List<List<int>> mainElements,
            bool onlyHighlight = false,
            List<int> newMainElement = null)
        {
            List<MouseButtonEventHandler> handlers = new List<MouseButtonEventHandler>();

            var mainElement = new List<int>();

            if (mainElements.Count > 0)
            {
                var tcs = new TaskCompletionSource<bool>();

                foreach (var child in simplexTable.Canvas.Children)
                {
                    if (child is System.Windows.Controls.Label label)
                    {
                        foreach (var pair in mainElements)
                        {
                            string targetUid = $"{pair[0]},{pair[1]}";

                            if (label.Uid == targetUid)
                            {
                                label.Background = new SolidColorBrush(Color.FromArgb(100, 0, 0, 255));
                                MouseButtonEventHandler mouseDownHandler = null;
                                mouseDownHandler = (sender, e) =>
                                {
                                    label.Background = new SolidColorBrush(Color.FromArgb(100, 255, 0, 0));
                                    mainElement.Add(pair[0]);
                                    mainElement.Add(pair[1]);
                                    label.MouseDown -= mouseDownHandler;
                                    tcs.SetResult(true);

                                };

                                label.MouseDown += mouseDownHandler;
                                handlers.Add(mouseDownHandler);
                            }
                        }
                    }
                }

                if (!onlyHighlight)
                    await tcs.Task;

            }

            foreach (var child in simplexTable.Canvas.Children)
            {
                if (child is System.Windows.Controls.Label label)
                {

                    foreach (var pair in mainElements)
                    {
                        string targetUid = $"{pair[0]},{pair[1]}";

                        if (label.Uid == targetUid)
                        {
                            if (newMainElement != null && pair.SequenceEqual(newMainElement))
                            {
                                label.Background = new SolidColorBrush(Color.FromArgb(100, 255, 0, 0));

                            }
                            foreach (MouseButtonEventHandler handler in handlers)
                                label.MouseDown -= handler;

                            MouseButtonEventHandler mouseDownHandler = null;
                            mouseDownHandler = (sender, e) =>
                            {
                                List<int> newMainElement = label.Uid.Split(',').Select(int.Parse).ToList();
                                label.MouseDown -= mouseDownHandler;
                                PreviousStep?.Invoke(newMainElement, simplexTable);
                            };
                            if (pair.SequenceEqual(mainElement))
                            {
                                label.Background = new SolidColorBrush(Color.FromArgb(100, 255, 0, 0));
                            }
                            else
                            {
                                label.MouseDown += mouseDownHandler;
                            }
                        }
                    }
                }
            }



            return mainElement;
        }

        /// <summary>
        /// Нахождение всех возможных опорных элементов
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        public static List<List<int>> findMainElements(SimplexTable simplexTable)
        {
            List<List<Fraction>> table = simplexTable.table;
            List<List<int>> mainElements = new List<List<int>>();
            List<Fraction> minimums = new List<Fraction>();
            int last = table.Count - 1;
            for (int i = 0; i != table[last].Count - 1; i++)
            {
                int? row = null; int? column = null;
                Fraction minimum = null;
                if (table[last][i] < 0)
                {
                    for (int j = 0; j != table.Count - 1; j++)
                    {

                        if (table[j][i] <= 0)
                            continue;
                        if (table[j][table[j].Count - 1] < 0)
                            continue;
                        column = i;
                        if (table[j][table[j].Count - 1] / table[j][i] < minimum || minimum == null)
                        {
                            minimum = table[j][table[j].Count - 1] / table[j][i];

                            row = j;
                            column = i;

                        }
                    }
                }
                if (row != null && column != null)
                {
                    mainElements.Add(new List<int> { (int)row, (int)column });
                }
                minimums.Add(minimum);
            }
            for (int i = 0; i != table[last].Count - 1; i++)
            {
                Fraction minimum = minimums[i];
                if (table[last][i] < 0)
                {
                    for (int j = 0; j != table.Count - 1; j++)
                    {

                        if (table[j][i] <= 0)
                            continue;
                        if (table[j][table[j].Count - 1] < 0)
                            continue;
                        if ((table[j][table[j].Count - 1] / table[j][i]).isEqualTo(minimum) && !mainElements.Any(list => list.SequenceEqual(new List<int> { (int)j, (int)i })))
                        {
                            mainElements.Add(new List<int> { (int)j, (int)i });
                        }

                    }
                }
            }

            bool idleStepPerformed = false;
            if (simplexTable.artificialTable && mainElements.Count <= 0)
                if (table[last].Last() == 0)
                {
                    if (ContainsArtificial(simplexTable.basisList, simplexTable.xAmount, simplexTable.conditions.Count))
                    {
                        for (int i = 0; i != table.Count - 1; i++)
                        {
                            if (simplexTable.basisList[i] >= simplexTable.xAmount - simplexTable.conditions.Count)
                            {
                                for (int j = 0; j != table[i].Count - 1; j++)
                                {
                                    if (table[i][j] != 0 && simplexTable.freeList[j] < simplexTable.xAmount - simplexTable.conditions.Count)
                                    {
                                        mainElements.Add(new List<int> { i, j });
                                        idleStepPerformed = true;
                                    }
                                }
                                if (!idleStepPerformed)
                                {
                                    simplexTable.basisList = simplexTable.basisList.Where((_, index) => index != i).ToArray();
                                    simplexTable.table.RemoveAt(i);
                                    break;

                                }
                            }
                        }
                    }
                }

            return mainElements;
        }
        /// <summary>
        /// Функция создания из линейной функции и списка линейных условий Симплекс таблицу с искуственным базисом
        /// </summary>
        /// <param name="function">Линейная функция</param>
        /// <param name="conditions">Линейные равенства</param>
        /// <returns>Симплекс таблица с искуственным базисом</returns>
        public static SimplexTable FormToAritificialBasis(LinearFunction function, List<LinearCondition> conditions)
        {
            int xAmount = function.xAmount;

            List<int> localBasis = new List<int>();

            for (int i = 0; i != conditions.Count; i++)
            {
                localBasis.Add(xAmount + i);
            }

            List<Fraction> functionList = new List<Fraction>();
            for (int i = 0; i != xAmount; i++)
            {
                functionList.Add(new Fraction(0));
            }
            for (int i = 0; i != conditions.Count; i++)
            {
                functionList.Add(new Fraction(1));
            }
            functionList.Add(new Fraction(0));

            LinearFunction functionF = new LinearFunction(xAmount + conditions.Count, functionList);



            List<LinearCondition> conditionsF = new List<LinearCondition>();
            for (int i = 0; i != conditions.Count; i++)
            {
                List<Fraction> condition = conditions[i].getData();
                if (condition.Last() < 0)
                {
                    for (int j = 0; j < condition.Count; j++)
                    {
                        condition[j] *= new Fraction(-1);
                    }
                }
                Fraction fx = condition.Last();
                condition.RemoveAt(condition.Count - 1);
                for (int j = 0; j != conditions.Count; j++)
                {
                    if (i == j)
                    {
                        condition.Add(new Fraction(1));
                    }
                    else
                    {
                        condition.Add(new Fraction(0));
                    }
                }
                condition.Add(fx);
                conditionsF.Add(new LinearCondition(conditions[0].xAmount + conditions.Count, condition));
            }
            Utils.doGauss(new Matrix(conditionsF), localBasis);

            SimplexTable smt = new SimplexTable(conditionsF, functionF, localBasis.ToArray());

            smt.artificialTable = true;

            return smt;



        }
        /// <summary>
        /// Функция восстановления плана из симплекс таблицы
        /// </summary>
        /// <param name="simplexTable">Симплекс таблица</param>
        /// <returns>Массив чисел плана данной таблицы</returns>
        public static Fraction[] makePlan(SimplexTable simplexTable)
        {
            int lastRow = simplexTable.table.Count - 1;
            int lastColumn = simplexTable.table[lastRow].Count - 1;


            int count = simplexTable.freeList.Length + simplexTable.basisList.Length;
            Fraction[] outputPlan = new Fraction[count];
            int c = 0;
            bool exc = false;
            for (int i = 0; i != count; i++)
            {

                if (simplexTable.freeList.Contains(i))
                {
                    outputPlan[i] = new Fraction(0);

                }
                else
                {
                    try
                    {
                        outputPlan[simplexTable.basisList[c]] = (simplexTable.table[c][lastColumn]);
                    }
                    catch (Exception)
                    {
                        exc = true;
                    }
                    c++;
                }
            }
            if (exc)
            {
                outputPlan = outputPlan.Select(item => item == default(Fraction) ? new Fraction(0) : item).ToArray();


            }
            return outputPlan;
        }
        /// <summary>
        /// Находит удаленные строки симплекс методом<br></br>
        /// Если удалено более 1 строки с
        /// </summary>
        /// <param name="simplexTable">Симплекс таблица с искуственным базисом</param>
        /// <returns>Возвращает номер переменной у которой удалилили строчку</returns>
        public static List<int> findRemoved(SimplexTable simplexTable, int xAmount = 0)
        {
            int count;
            if (xAmount != 0)
                count = xAmount;
            else
                count = simplexTable.xAmount;
            List<int> removed = new List<int>();
            for (int j = 0; j != count; j++)
            {
                if (!simplexTable.basisList.Contains(j) && !simplexTable.freeList.Contains(j))
                {
                    removed.Add(j);
                }
            }
            if (removed.Count == 0)
                return null;
            else
                return removed;
        }
        /// <summary>
        /// Асинхронная функция совершения итерации симплекс таблицы
        /// </summary>
        /// <param name="simplexTable">Симплекс таблица в которой нужно сделать итерацию</param>
        /// <returns>Симплекс таблица после итерации, null если итерацию сделать невозможно</returns>
        public async static Task<SimplexTable> DoSimplexIteration(SimplexTable simplexTable, List<int> newMainElement = null)
        {
            SimplexTable output = new SimplexTable(simplexTable.table,
                simplexTable.basisList,
                simplexTable.freeList,
                simplexTable.iter + 1,
                simplexTable.artificialTable);


            List<List<int>> mainElements = findMainElements(output);
            List<int> mainElement = new List<int>();
            int? xs = null; int? xr = null;

            if (mainElements.Count > 0)
            {
                if (!AutoSimplex)
                {
                    if (newMainElement != null)
                    {
                        xs = newMainElement[0];
                        xr = newMainElement[1];
                        await HighlightMainElements(simplexTable, mainElements, true, newMainElement);
                        newMainElement = null;
                    }
                    else
                    {
                        mainElement = await HighlightMainElements(simplexTable, mainElements);
                        xs = mainElement[0];
                        xr = mainElement[1];
                    }
                }
                else
                {
                    int max = 0;
                    bool performed = false;
                    for (int i = 0; i != mainElements.Count; i++)
                    {
                        if (output.basisList.ToList().Contains(output.basisList[mainElements[i][0]]) && max < output.basisList[mainElements[i][0]])
                        {
                            max = output.basisList[mainElements[i][0]];
                            mainElement = mainElements[i];
                            xs = mainElement[0];
                            xr = mainElement[1];
                            performed = true;
                        }
                    }
                    if (!performed)
                    {
                        mainElement = mainElements[0];
                        xs = mainElement[0];
                        xr = mainElement[1];
                    }
                }
            }
            if (xs == null || xr == null)
            {
                if (CheckSimplexTable(output))
                {
                    if (output.table.Count == output.conditions.Count)
                    {
                        List<Fraction> outputList = new List<Fraction>();
                        outputList.Add(new Fraction(findRemoved(output)[0] - (output.xAmount - output.conditions.Count) + 1));

                        SimplexOperation?.Invoke("idleStep", outputList);
                        return output;
                    }
                    else if (output.artificialTable && output.table.Last().Last() != 0)
                    {
                        SimplexOperation?.Invoke("wrong", null);
                        return null;
                    }
                    SimplexOperation?.Invoke("end", makePlan(output).ToList());

                }

                return null;
            }

            int newXS = (int)xs;
            int newXR = (int)xr;
            SimplexOperation?.Invoke("mainElement", new List<Fraction> { new Fraction(newXS + 1), new Fraction(newXR + 1) });


            int[] newFreeList = new int[output.freeList.Length];
            output.freeList.CopyTo(newFreeList, 0);
            int[] newBasisList = new int[output.basisList.Length];
            output.basisList.CopyTo(newBasisList, 0);

            newBasisList[newXS] = output.freeList[newXR];
            newFreeList[newXR] = output.basisList[newXS];

            Fraction baseElement = output.table[newXS][newXR];

            output.table[newXS][newXR] = Fraction.swap(baseElement);
            for (int i = 0; i != output.table[newXS].Count; i++)
            {
                if (i == newXR)
                    continue;
                output.table[newXS][i] = output.table[newXS][i] / baseElement;
            }
            for (int i = 0; i != output.table.Count; i++)
            {
                if (i == newXS)
                    continue;
                output.table[i][newXR] = output.table[i][newXR] / (baseElement * (new Fraction(-1)));
            }
            for (int i = 0; i != output.table.Count; i++)
            {
                if (i == newXS)
                    continue;
                for (int j = 0; j != output.table[i].Count; j++)
                {
                    if (j == newXR)
                        continue;
                    output.table[i][j] -= output.table[i][newXR] * (baseElement * (new Fraction(-1)) * (output.table[newXS][j]));
                }

            }
            output.basisList = newBasisList;
            output.freeList = newFreeList;
            output.table = output.table;

            return output;

        }
        /// <summary>
        /// Функция проверки искуственного базиса симплекс таблицы
        /// </summary>
        /// <param name="basisList">Список базисных переменных</param>
        /// <param name="xAmount">Количество переменных</param>
        /// <param name="conditionsCount">Количество условий</param>
        /// <returns>True если содержит переменные искуственного базиса<br></br>
        /// False если не содержит</returns>
        public static bool ContainsArtificial(int[] basisList, int xAmount, int conditionsCount)
        {
            if (basisList == null)
                return false;
            for (int i = 0; i != conditionsCount; i++)
            {
                if (basisList.Contains(xAmount - 1 - i))
                    return true;
            }
            if (basisList.Where(x => x >= xAmount).Count() != 0)
                return true;
            return basisList.All(x => x == 0) && basisList.Length != 1;
        }
        /// <summary>
        /// Функция проверки симплекс таблицы<br></br>
        /// Отправляет событие "wrong" если матрица не нашлось опорных элементов<br></br>
        /// Хотя нижняя строчка все еще содержит отрицательные значения<br></br>
        /// И возвращает false<br></br>
        /// Отправляет событие "basis" если последний столбец содержит отрицательные значения<br></br>
        /// И возвращает false<br></br>
        /// Иначе возвращает true
        /// </summary>
        /// <param name="simplexTable">Симплекс таблица которую нужно проверить</param>
        /// <returns>Успешно ли завершилась симплекс таблица</returns>
        public static bool CheckSimplexTable(SimplexTable simplexTable)
        {
            int lastRow = simplexTable.table.Count - 1;
            int lastColumn = simplexTable.table[lastRow].Count - 1;


            for (int i = 0; i != simplexTable.table[lastRow].Count - 1; i++)
            {
                if (simplexTable.table[lastRow][i] < 0)
                {
                    List<List<int>> mainElements = findMainElements(simplexTable);
                    if (mainElements.Count == 0)
                    {
                        SimplexOperation?.Invoke("wrong", null);
                        return false;
                    }
                }
            }
            for (int i = 0; i != simplexTable.table.Count - 1; i++)
            {
                if (simplexTable.table[i][lastColumn] < 0)
                {
                    SimplexOperation?.Invoke("basis", null);
                    return false; ;
                }

            }

            return true;
        }
        /// <summary>
        /// Автоматически находит базис через искуственный симплекс метод
        /// </summary>
        /// <param name="conditions">Линейные уравнения</param>
        /// <returns>Симплекс таблицу финальную, если null значит не нашло</returns>
        public async static Task<SimplexTable> AutoFindBasisWithSimplex(List<LinearCondition> conditions)
        {
            int xAmount = conditions[0].xAmount;

            List<Fraction> newFunctionData = new List<Fraction>();

            for (int i = 0; i != xAmount + 1; i++)
            {
                newFunctionData.Add(new Fraction(0));
            }

            SimplexTable simplexTable = FormToAritificialBasis(new LinearFunction(xAmount, newFunctionData), conditions);


            int[] localBasis = new int[xAmount];

            if (simplexTable != null)
                localBasis = simplexTable.basisList;
            else
                return null;

            while (Utils.ContainsArtificial(simplexTable.basisList, simplexTable.xAmount, conditions.Count))
            {

                simplexTable = await Utils.DoSimplexIteration(simplexTable);

                if (simplexTable != null)
                {
                    localBasis = simplexTable.basisList;
                }
                else
                    break;
            }

            if (simplexTable == null && Utils.ContainsArtificial(localBasis, xAmount, conditions.Count))
            {
                return null;
            }

            return simplexTable;
        }
        /// <summary>
        /// Удаление линейнозависимых строк из равенств путем запуска симплекс метода
        /// </summary>
        /// <param name="conditions">Уравнения в которых нужно проверить линейную зависимость</param>
        /// <returns>Возвращает список равенств без линейной зависимости</returns>
        public async static Task<List<LinearCondition>> RemoveDependentLines(List<LinearCondition> conditions)
        {
            SimplexTable artificial = await AutoFindBasisWithSimplex(conditions);
            if (artificial == null)
                return conditions;
            int xAmount = artificial.xAmount - conditions.Count;
            List<LinearCondition> newConditions = conditions.ToList();
            if (artificial.basisList.Length != conditions.Count)
            {
                List<int> removed = Utils.findRemoved(artificial);
                if (removed != null)
                {
                    removed.Sort();
                    removed.Reverse();
                    for (int i = 0; i < removed.Count; i++)
                    {
                        newConditions.RemoveAt(removed[i] - xAmount);
                    }
                }
            }
            return newConditions;
        }

        #endregion SimplexMethod

        #region GraphicUtils
        /// <summary>
        /// Проверяет ограничена ли область решений путем запуска симплекс метода
        /// </summary>
        /// <param name="graphic">График со списком фукнкций внутри</param>
        /// <returns>True если ограничена, False если нет</returns>
        public static async Task<bool> isRestricted(Graphic graphic)
        {
            List<LinearCondition> newConditions = new List<LinearCondition>();
            int xAmount = graphic.function.xAmount;
            int addX = graphic.conditionLines.Count(x => x.conditionType != ConditionType.Equal && x.color != Brushes.Transparent);
            graphic.conditionLines = graphic.conditionLines
            .OrderBy(line => line.conditionType == ConditionType.Equal) // False (not Equal) goes first, True (Equal) goes second
            .ToList();

            for (int i = 0; i != graphic.conditionLines.Count(x => x.color != Brushes.Transparent); i++)
            {

                LinearCondition condition = (LinearCondition)graphic.conditionLines[i].linearCondition;
                List<Fraction> data = condition.getData();
                if (condition.conditionType == ConditionType.MoreOrEqualThen)
                {
                    for (int j = 0; j != data.Count; j++)
                    {
                        data[j] *= new Fraction(-1);
                    }
                }
                Fraction c = data.Last();
                data.RemoveAt(data.Count - 1);
                for (int j = 0; j != addX; j++)
                {
                    if (j == i)
                        data.Add(new Fraction(1));
                    else
                        data.Add(new Fraction(0));
                }
                data.Add(c);

                newConditions.Add(new LinearCondition(xAmount + addX, data, condition.conditionType));
            }

            List<Fraction> newFunctionData = graphic.function.GetRawData();
            Fraction last = newFunctionData.Last();
            newFunctionData.RemoveAt(newFunctionData.Count - 1);
            for (int i = 0; i != graphic.conditionLines.Count(x => x.color != Brushes.Transparent); i++)
            {
                newFunctionData.Add(new Fraction(0));
            }
            newFunctionData.Add(last);
            LinearFunction newFunction = new LinearFunction(xAmount + addX, newFunctionData, graphic.function.min);
            SimplexTable artificial = Utils.FormToAritificialBasis(newFunction, newConditions);

            int[] localBasis = new int[newFunction.xAmount + newConditions.Count];

            if (artificial != null)
                localBasis = artificial.basisList;
            else
                return false;
            xAmount = artificial.xAmount;
            while (Utils.ContainsArtificial(artificial.basisList, xAmount, newConditions.Count))
            {

                artificial = await Utils.DoSimplexIteration(artificial);

                if (artificial != null)
                {
                    localBasis = artificial.basisList;
                }
                else
                    break;
            }

            if (artificial == null && Utils.ContainsArtificial(localBasis, xAmount + addX, newConditions.Count))
            {
                return false;
            }

            localBasis = artificial.basisList;
            List<LinearCondition> finalConditions = newConditions.ToList();
            if (localBasis.Length != newConditions.Count)
            {
                List<int> removed = Utils.findRemoved(artificial, xAmount);
                if (removed != null)
                {
                    removed.Sort();
                    removed.Reverse();
                    for (int i = 0; i < removed.Count; i++)
                    {
                        finalConditions.RemoveAt(removed[i] - (xAmount - newConditions.Count));
                    }
                }
            }


            newFunctionData = graphic.function.GetRawData();
            last = newFunctionData.Last();
            newFunctionData.RemoveAt(newFunctionData.Count - 1);
            for (int i = 0; i != addX; i++)
            {
                newFunctionData.Add(new Fraction(0));
            }
            newFunctionData.Add(last);
            newFunction = new LinearFunction(graphic.function.xAmount + addX, newFunctionData, graphic.function.min);

            SimplexTable simplexTable = new SimplexTable(newConditions, newFunction, localBasis);

            if (!Utils.CheckSimplexTable(simplexTable))
            {
                return false;
            }

            SimplexTable nextSimplexTable = await Utils.DoSimplexIteration(simplexTable);
            while (nextSimplexTable != null)
            {
                if (!Utils.CheckSimplexTable(nextSimplexTable))
                {
                    return false;
                }
                nextSimplexTable = await Utils.DoSimplexIteration(nextSimplexTable);
            }
            return true;
        }
        /// <summary>
        /// Функция поиска расстояния между точками
        /// </summary>
        /// <param name="firstPoint">Первая точка</param>
        /// <param name="secondPoint">Вторая точка</param>
        /// <returns>Расстояние между точками</returns>
        public static double FindDistance(Point firstPoint, Point secondPoint)
        {
            return Math.Sqrt(Math.Pow((secondPoint.X - firstPoint.X), 2) + Math.Pow((secondPoint.Y - firstPoint.Y), 2));
        }
        /// <summary>
        /// Функция построения выпуклой оболочки из списка точек
        /// </summary>
        /// <param name="points">Точки которые нужно отсортировать</param>
        /// <returns>Отсортированный список точек образующий выпуклую оболочку</returns>
        public static List<FractionPoint> ConvexHull(List<FractionPoint> points)
        {
            if (points.Count <= 1)
                return points;

            FractionPoint leftmost = points.OrderBy(p => p.X.Value()).ThenBy(p => p.Y.Value()).First();
            List<FractionPoint> hull = new List<FractionPoint>();

            FractionPoint current = leftmost;

            do
            {
                hull.Add(current);

                FractionPoint next = points[0];

                foreach (var candidate in points)
                {
                    if (candidate == current) continue;

                    double cross = CrossProduct(current, next, candidate);

                    if (cross > 0 || (cross == 0 && Utils.FindDistance(current, candidate) > Utils.FindDistance(current, next)))
                    {
                        next = candidate;
                    }
                }

                current = next;

            } while (current != leftmost);

            return hull;
        }
        /// <summary>
        /// Вычисляет векторное произведение двух векторов AB и AC, заданных точками A, B и C.
        /// </summary>
        /// <param name="A">Начальная точка обоих векторов (точка A)</param>
        /// <param name="B">Конечная точка первого вектора (точка B)</param>
        /// <param name="C">Конечная точка второго вектора (точка C)</param>
        /// <returns>
        /// Положительное значение — точка C находится слева от вектора AB (по часовой стрелке). <br />
        /// Отрицательное значение — точка C находится справа от вектора AB (против часовой стрелки). <br />
        /// Нулевое значение — точки A, B и C лежат на одной прямой (коллинеарны).
        /// </returns>
        public static double CrossProduct(Point A, Point B, Point C)
        {
            return (B.X - A.X) * (C.Y - A.Y) - (B.Y - A.Y) * (C.X - A.X);
        }
        /// <summary>
        /// Вычисляет концы списка точек которые находятся на одной прямой
        /// </summary>
        /// <param name="points">Список коллинеарных точек</param>
        /// <returns>Список из двух концевых точек</returns>
        public static List<FractionPoint> FindEdges(List<FractionPoint> points)
        {
            double distance = 0;
            FractionPoint firstPoint = new FractionPoint();
            FractionPoint secondPoint = new FractionPoint();
            for (int i = 0; i != points.Count - 1; i++)
            {
                for (int j = i + 1; j != points.Count; j++)
                {
                    double newDistance = Utils.FindDistance(points[i], points[j]);
                    if (distance < newDistance)
                    {
                        firstPoint = points[i];
                        secondPoint = points[j];
                        distance = newDistance;
                    }
                }
            }
            return new List<FractionPoint> { firstPoint, secondPoint };

        }

        #endregion


    }
}
