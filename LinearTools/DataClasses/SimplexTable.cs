using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace LinearTools
{
    public class SimplexTable
    {
        /// <summary>
        /// Количество переменных
        /// </summary>
        public int xAmount { get; set; }
        /// <summary>
        /// Массив базисных переменных
        /// </summary>
        public int[] basisList { get; set; }
        /// <summary>
        /// Массив свободных переменных
        /// </summary>
        public int[] freeList { get; set; }
        /// <summary>
        /// Функция в виде списка
        /// </summary>
        public List<Fraction> function { get; set; }
        /// <summary>
        /// Равенства в виде списка списков
        /// </summary>
        public List<List<Fraction>> conditions { get; set; }
        /// <summary>
        /// Симплекс таблица в виде списка списков
        /// </summary>
        public List<List<Fraction>> table { get; set; }
        /// <summary>
        /// Номер итерации (для отрисовки)
        /// </summary>
        public int iter { get; set; }
        /// <summary>
        /// Холст для отрисовки симплекс таблицы
        /// </summary>
        public Canvas Canvas { get; set; }
        /// <summary>
        /// Таблица с искуственным базисом ли
        /// </summary>
        public bool artificialTable { get; set; }


        /// <summary>
        /// Конструктор первоначальной инициализации
        /// </summary>
        /// <param name="conditions">Список равенств типа LinearCondition</param>
        /// <param name="function">Функция типа LinearFunction</param>
        /// <param name="basisList">Список базисных переменных</param>
        public SimplexTable(List<LinearCondition> conditions, LinearFunction function, int[] basisList)
        {
            this.conditions = new List<List<Fraction>>();
            for (int i = 0; i != conditions.Count; i++)
            {
                this.conditions.Add(conditions[i].getData());
            }
            this.function = function.GetData();
            this.xAmount = conditions[0].xAmount;
            this.basisList = basisList;
            this.artificialTable = false;
            initWithConditions();
            iter = 0;

        }
        /// <summary>
        /// Конструктор для следующих итераций
        /// </summary>
        /// <param name="table">Симплекс таблица в виде списка списков</param>
        /// <param name="basisList">Список базисных переменных</param>
        /// <param name="freeList">Список свободных переменных</param>
        /// <param name="iter">Необязательный номер итерации, по умолчанию ноль</param>
        public SimplexTable(List<List<Fraction>> table, int[] basisList, int[] freeList, int iter = 0, bool artificialTable = false)
        {

            this.table = table.Select(row => row.Select(cell => new Fraction(cell)).ToList()).ToList();

            this.basisList = (int[])basisList.Clone();
            this.freeList = (int[])freeList.Clone();

            this.xAmount = this.basisList.Length + this.freeList.Length;

            this.function = this.table[this.table.Count - 1].Select(cell => new Fraction(cell)).ToList();

            this.conditions = this.table.GetRange(0, this.table.Count - 1);

            this.iter = iter;

            this.artificialTable = artificialTable;
        }

        /// <summary>
        /// Инициализация первоначальной симплекс таблицы
        /// </summary>
        private void initWithConditions()
        {
            Matrix initialMatrix = new Matrix(conditions);
            if (Utils.isRight(initialMatrix))
            {
                throw new Exception("Система несовместна");
            }
            List<int> basisList = new List<int>();
            for (int i = 0; i != this.basisList.Length; i++)
            {
                basisList.Add(this.basisList[i]);
            }

            List<Fraction> fx = new List<Fraction>();
            for (int i = 0; i != function.Count; i++)
            {
                fx.Add(new Fraction(function[i]));
            }
            List<int> freeList = new List<int>();
            for (int i = 0; i != initialMatrix.Column; i++)
            {
                if (!basisList.Contains(i))
                {
                    freeList.Add(i);
                }
            }

            Matrix formedMatrix = Utils.doGauss(initialMatrix, basisList);

            if (!Utils.isLineryIndependent(formedMatrix, basisList))
            {
                throw new Exception("Система линейно зависима");

            }

            table = new List<List<Fraction>>();

            for (int i = 0; i != formedMatrix.Conditions.Count; i++)
            {
                table.Add(new List<Fraction>());
                for (int j = 0; j != formedMatrix.Conditions[i].Count; j++)
                {
                    if (basisList.Contains(j))
                        continue;
                    table[i].Add(formedMatrix.Conditions[i][j]);
                }
            }

            for (int i = 0; i != basisList.Count; i++)
            {
                if (fx[basisList[i]] != 0)
                {
                    Fraction koeff = fx[basisList[i]];
                    for (int j = 0; j != fx.Count; j++)
                    {
                        fx[j] -= formedMatrix.Conditions[i][j] * koeff;
                    }
                }
            }

            table.Add(new List<Fraction>());
            for (int i = 0; i != fx.Count; i++)
            {
                if (!basisList.Contains(i))
                {
                    table[table.Count - 1].Add(fx[i]);
                }
            }
            int[] newBasisList = new int[basisList.Count];
            basisList.CopyTo(newBasisList, 0);
            int[] newFreeList = new int[freeList.Count];
            freeList.CopyTo(newFreeList, 0);

            this.basisList = newBasisList;
            this.freeList = newFreeList;

        }

        /// <summary>
        /// Отрисовка симплекс таблицы на холст
        /// </summary>
        public void drawSimplexTable()
        {
            double[] maxColumnWidths = new double[table[0].Count];

            for (int i = 0; i < table.Count; i++)
            {
                List<Fraction> dataLine = table[i];
                for (int j = 0; j < table[0].Count; j++)
                {
                    Label tempLabel = new Label();
                    tempLabel.Content = dataLine[j];
                    tempLabel.FontSize = 16;
                    tempLabel.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

                    maxColumnWidths[j] = Math.Max(maxColumnWidths[j], tempLabel.DesiredSize.Width + 120);
                }
            }


            Canvas = new Canvas();
            Canvas.Height = 0;
            Canvas.Width = 0;
            Canvas.HorizontalAlignment = HorizontalAlignment.Left;

            Line line = new Line();

            Label iteration = new Label();
            iteration.Content = "x" + Utils.makeUpperIndex(iter);
            iteration.FontSize = 25;
            iteration.Height = 50;
            iteration.Width = 50;
            iteration.VerticalContentAlignment = VerticalAlignment.Center;
            iteration.HorizontalContentAlignment = HorizontalAlignment.Center;

            Canvas.Children.Add(iteration);
            Canvas.Height += iteration.Height;
            Canvas.Width += iteration.Width;

            for (int i = 0; i < basisList.Length; i++)
            {
                Label x = new Label();
                x.Content = "x" + Utils.makeLowerIndex(basisList[i] + 1);
                x.FontSize = 25;
                x.Height = 50;
                x.Width = 50;
                x.VerticalContentAlignment = VerticalAlignment.Center;
                x.HorizontalContentAlignment = HorizontalAlignment.Center;
                Canvas.SetTop(x, 50 + 50 * i);
                Canvas.Children.Add(x);
                Canvas.Height += iteration.Height;
            }

            Label F = new Label();
            F.Content = "F";
            F.FontSize = 25;
            F.Height = 50;
            F.Width = 50;
            F.VerticalContentAlignment = VerticalAlignment.Center;
            F.HorizontalContentAlignment = HorizontalAlignment.Center;
            Canvas.SetTop(F, Canvas.Height);
            Canvas.Children.Add(F);

            Canvas.Height += F.Height;

            for (int i = 0; i < freeList.Length; i++)
            {
                Label x = new Label();
                x.Content = "x" + Utils.makeLowerIndex(freeList[i] + 1);
                x.FontSize = 25;
                x.Height = 50;
                x.Width = maxColumnWidths[i];
                x.VerticalContentAlignment = VerticalAlignment.Center;
                x.HorizontalContentAlignment = HorizontalAlignment.Center;
                Canvas.SetLeft(x, Canvas.Width);
                Canvas.Children.Add(x);
                Canvas.Width += x.Width;
            }
            Label B = new Label();
            B.Content = "B";
            B.FontSize = 25;
            B.Height = 50;
            B.Width = maxColumnWidths[maxColumnWidths.Length - 1];
            B.VerticalContentAlignment = VerticalAlignment.Center;
            B.HorizontalContentAlignment = HorizontalAlignment.Center;
            Canvas.SetLeft(B, Canvas.Width);

            Canvas.Children.Add(B);

            Canvas.Width += B.Width;


            for (int i = 0; i < basisList.Length + 1; i++)
            {
                line = new Line();
                line.X1 = 0;
                line.X2 = Canvas.Width;
                line.Y1 = 0;
                line.Y2 = 0;
                line.Stroke = Brushes.Black;
                line.StrokeThickness = 2;

                Canvas.SetTop(line, 50 + 50 * i);

                Canvas.Children.Add(line);
            }

            double currentX = 0;
            for (int i = 0; i < freeList.Length + 1; i++)
            {
                line = new Line();
                line.X1 = 0;
                line.X2 = 0;
                line.Y1 = 0;
                line.Y2 = Canvas.Height;
                line.Stroke = Brushes.Black;
                line.StrokeThickness = 2;

                Canvas.SetLeft(line, 50 + currentX);

                Canvas.Children.Add(line);
                currentX += maxColumnWidths[i];

            }
            for (int i = 0; i != table.Count; i++)
            {
                currentX = 50;
                for (int j = 0; j != table[i].Count; j++)
                {
                    Label x = new Label();
                    x.Content = table[i][j];
                    x.FontSize = 20;
                    x.Width = maxColumnWidths[j];
                    x.Height = 50;
                    x.VerticalContentAlignment = VerticalAlignment.Center;
                    x.HorizontalContentAlignment = HorizontalAlignment.Center;
                    x.Uid = i.ToString() + "," + j.ToString();

                    Canvas.SetTop(x, 50 + 50 * i);
                    Canvas.SetLeft(x, currentX);
                    currentX += maxColumnWidths[j];

                    Canvas.Children.Add(x);

                }
            }

            Line Top = new Line();
            Top.X1 = 0;
            Top.X2 = Canvas.Width;
            Top.Y1 = 0;
            Top.Y2 = 0;
            Top.StrokeThickness = 2;
            Top.Stroke = Brushes.Black;

            Line Bot = new Line();
            Bot.X1 = 0;
            Bot.X2 = Canvas.Width;
            Bot.Y1 = Canvas.Height;
            Bot.Y2 = Canvas.Height;
            Bot.StrokeThickness = 2;
            Bot.Stroke = Brushes.Black;

            Line Left = new Line();
            Left.X1 = 0;
            Left.X2 = 0;
            Left.Y1 = 0;
            Left.Y2 = Canvas.Height;
            Left.StrokeThickness = 2;
            Left.Stroke = Brushes.Black;

            Line Right = new Line();
            Right.X1 = Canvas.Width;
            Right.X2 = Canvas.Width;
            Right.Y1 = 0;
            Right.Y2 = Canvas.Height;
            Right.StrokeThickness = 2;
            Right.Stroke = Brushes.Black;

            Canvas.Children.Add(Top);
            Canvas.Children.Add(Bot);
            Canvas.Children.Add(Left);
            Canvas.Children.Add(Right);
        }

        /// <summary>
        /// Функция копирования симплелкс таблицы
        /// </summary>
        /// <returns></returns>
        public SimplexTable Copy()
        {
            return new SimplexTable(table, basisList, freeList, iter, artificialTable);
        }

        /// <summary>
        /// Оператор при обращении к таблице в виде UIElement возвращает её холст
        /// </summary>
        public static implicit operator UIElement(SimplexTable simplexTable) => simplexTable.Canvas;
    }
}
