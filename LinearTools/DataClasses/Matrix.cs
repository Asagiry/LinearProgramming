using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace LinearTools
{
    public class Matrix
    {
        /// <summary>
        /// Холст на котором матрица
        /// </summary>
        public Canvas Canvas { get; set; }
        /// <summary>
        /// Количество столбцов
        /// </summary>
        public int Column { get; set; }
        /// <summary>
        /// Количество строк
        /// </summary>
        public int Row { get; set; }
        /// <summary>
        /// Матрица выражений
        /// </summary>
        public List<List<Fraction>> Conditions { get; set; }


        /// <summary>
        /// Конструктор из списка LinearConditions
        /// </summary>
        /// <param name="conditions">Список линейных равенств</param>
        public Matrix(List<LinearCondition> conditions)
        {
            Column = conditions[0].getData().Count - 1;
            Row = conditions.Count;

            Conditions = new List<List<Fraction>>();
            for (int i = 0; i < Row; i++)
            {
                List<Fraction> dataLine = conditions[i].getData();
                Conditions.Add(dataLine);
            }

        }
        /// <summary>
        /// Конструктор из списка списков Fraction
        /// </summary>
        /// <param name="conditions">Список списков Fraction</param>
        public Matrix(List<List<Fraction>> conditions)
        {
            Column = conditions[0].Count - 1;
            Row = conditions.Count;

            Conditions = new List<List<Fraction>>();
            for (int i = 0; i < Row; i++)
            {
                List<Fraction> dataLine = conditions[i];
                List<Fraction> newDataLine = new List<Fraction>(dataLine);
                Conditions.Add(newDataLine);
            }
        }
        public Matrix()
        {
            Canvas = null;
            Column = 0;
            Row = 0;
            Conditions = null;
        }
        /// <summary>
        /// Отрисовка матрицы на хослсте
        /// </summary>
        public void drawMatrix()
        {
            Canvas = new Canvas();
            Canvas.HorizontalAlignment = HorizontalAlignment.Left;
            Canvas.Height = 15;
            Canvas.Width = 15;

            double[] maxColumnWidths = new double[Column + 1];

            for (int i = 0; i < Row; i++)
            {
                List<Fraction> dataLine = Conditions[i];
                for (int j = 0; j <= Column; j++)
                {

                    Label tempLabel = new Label();
                    tempLabel.Content = dataLine[j];
                    tempLabel.FontSize = 16;
                    tempLabel.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

                    maxColumnWidths[j] = Math.Max(maxColumnWidths[j], tempLabel.DesiredSize.Width + 5);
                }
            }

            for (int i = 0; i < Row; i++)
            {
                List<Fraction> dataLine = Conditions[i];

                double currentX = 15;

                for (int j = 0; j <= Column; j++)
                {
                    Label a = new Label();
                    a.Content = dataLine[j];
                    a.FontSize = 16;
                    a.Height = 50;

                    a.Width = maxColumnWidths[j];
                    a.VerticalContentAlignment = VerticalAlignment.Bottom;
                    a.HorizontalContentAlignment = HorizontalAlignment.Right;

                    Canvas.SetLeft(a, currentX);
                    Canvas.SetTop(a, 50 * i);
                    Canvas.Children.Add(a);

                    currentX += a.Width;

                    if (j != dataLine.Count - 1)
                    {
                        Label x = new Label();
                        x.Content = "X" + Utils.makeLowerIndex(j + 1);
                        x.FontSize = 16;
                        x.Height = 50;
                        x.Width = 25;
                        x.VerticalContentAlignment = VerticalAlignment.Bottom;


                        Canvas.SetLeft(x, currentX);
                        Canvas.SetTop(x, 50 * i);
                        Canvas.Children.Add(x);
                        currentX += 30;
                        if (i == Row - 1 & j == dataLine.Count - 2)
                        {
                            Line line = new Line();
                            line.X1 = 0;
                            line.X2 = 0;
                            line.Y1 = 0;
                            line.Y2 = Canvas.Height + 50;
                            line.Stroke = Brushes.Black;
                            line.StrokeThickness = 2;

                            Canvas.SetLeft(line, currentX);
                            Canvas.SetTop(line, 5);

                            Canvas.Children.Add(line);
                        }

                    }


                }

                Canvas.Width = currentX + 15;
                Canvas.Height += 50;
            }
            Path leftArc = new Path
            {
                Stroke = Brushes.Black,
                StrokeThickness = 2,
                Data = new PathGeometry(new PathFigureCollection
                {
                    new PathFigure
                    {
                        StartPoint = new Point(10, 5), // Начальная точка (верхняя часть полукруга)
                        Segments = new PathSegmentCollection
                        {
                            new ArcSegment
                            {
                                Point = new Point(10, Canvas.Height - 5), // Конечная точка (нижняя часть полукруга)
                                Size = new Size(30, Canvas.Height / 2), // Размеры дуги
                                SweepDirection = SweepDirection.Counterclockwise // Направление дуги
                            }
                        }
                    }
                })
            };
            // Устанавливаем позицию дуги на Canvas
            Canvas.SetLeft(leftArc, 20);
            Canvas.SetTop(leftArc, 5);
            Canvas.Children.Add(leftArc);
            Path rightArc = new Path
            {
                Stroke = Brushes.Black,
                StrokeThickness = 2,
                Data = new PathGeometry(new PathFigureCollection
                {
                    new PathFigure
                    {
                        StartPoint = new Point(10, 5), // Начальная точка (верхняя часть полукруга)
                        Segments = new PathSegmentCollection
                        {
                            new ArcSegment
                            {
                                Point = new Point(10, Canvas.Height - 5), // Конечная точка (нижняя часть полукруга)
                                Size = new Size(30, Canvas.Height / 2), // Размеры дуги
                                SweepDirection = SweepDirection.Clockwise // Направление дуги
                            }
                        }
                    }
                })
            };
            // Устанавливаем позицию дуги на Canvas
            Canvas.SetLeft(rightArc, Canvas.Width - 30);
            Canvas.SetTop(rightArc, 5);
            Canvas.Children.Add(rightArc);
        }

        /// <summary>
        /// Функция копирования матрицы
        /// </summary>
        /// <returns></returns>
        public Matrix Copy()
        {
            // Копируем каждую строку, создавая новый список и новый объект Fraction для каждого элемента
            var copiedConditions = Conditions
                .Select(row => row.Select(fraction => new Fraction(fraction)).ToList())
                .ToList();

            // Возвращаем новую матрицу с копией всех строк
            return new Matrix(copiedConditions)
            {
                Column = this.Column,
                Row = this.Row,
            };
        }

        /// <summary>
        /// Оператор при обращении к матрицы в виде UIElement возвращает ее холст
        /// </summary>
        /// <param name="matrix"></param>
        public static implicit operator UIElement(Matrix matrix) => matrix.Canvas;
    }
}
