using LinearTools.GraphicMethod;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace LinearTools
{
    public class Graphic
    {
        /// <summary>
        /// Холст на котором весь график
        /// </summary>
        public Canvas Canvas { get; set; }
        /// <summary>
        /// Масштаб графика
        /// </summary>
        public double Scale { get; set; }
        /// <summary>
        /// Линейная функция графика
        /// </summary>
        public LinearFunction function { get; set; }
        /// <summary>
        /// Список всех точек пересечения
        /// </summary>
        private List<FractionPoint> points { get; set; }
        /// <summary>
        /// Индексы осевых переменных
        /// </summary>
        public List<int> Axes { get; set; }
        /// <summary>
        /// Список ограничений 
        /// </summary>
        public List<GraphicLine> conditionLines { get; set; }
        /// <summary>
        /// Линия уровня
        /// </summary>
        public GraphicLine functionLine { get; set; }
        /// <summary>
        /// Линия нормали
        /// </summary>
        private GraphicLine normalLine { get; set; }
        /// <summary>
        /// Центр по оси Х
        /// </summary>
        public double centerX { get; set; }
        /// <summary>
        /// Центр по оси Y
        /// </summary>
        public double centerY { get; set; }
        /// <summary>
        /// Ограничена ли область значений <br></br>
        /// false если нет, true если ограничена
        /// </summary>
        public bool isRestricted { get; set; }
        /// <summary>
        /// Отрисовывать ли все области при перерисовке графика
        /// </summary>
        public bool allAreas { get; set; }
        /// <summary>
        /// Конструктор графика 
        /// </summary>
        /// <param name="height">Высота графика</param>
        /// <param name="width">Ширина графика</param>
        /// <param name="scale">Исходный масштаб</param>
        /// <param name="axes">Индексы осевых переменных, если второй индекс == 0, график одномерный</param>
        public Graphic(double height, double width, double scale = 1, List<int> axes = null)
        {

            Canvas = new Canvas();
            Canvas.ClipToBounds = true;
            Scale = scale;
            Canvas.Height = height;
            Canvas.Width = width;
            conditionLines = new List<GraphicLine>();
            centerX = Canvas.Width / 2;
            centerY = Canvas.Height / 2;
            Axes = axes;
            isRestricted = false;
            DrawAxes();
        }

        /// <summary>
        /// Событие обработки изменения масштаба графика
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void GraphicScaleChange(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta < 0)
            {
                if (Scale > 10)
                    return;
                Scale = Math.Round(Math.Min(Scale + 0.1, 10), 2);
            }
            else
            {
                if (Scale < 0.2)
                    return;
                Scale = Math.Round(Math.Max(Scale - 0.1, 0.1), 2);
            }

            DrawAxes();
            if (conditionLines.Count > 0)
            {
                foreach (GraphicLine graphicLine in conditionLines)
                {
                    graphicLine.DrawLine(this);
                }
                DrawPolygram();
                if (function != null)
                    DrawMainFunction();

                foreach (GraphicLine line in conditionLines.Where(x => x.color != Brushes.Transparent))
                {
                    if (line.area != null)
                    {
                        line.DrawArea();
                    }
                }

            }
        }
        /// <summary>
        /// Отрисовка осей графика и функций на нем если они были заданы
        /// </summary>
        private void DrawAxes()
        {
            Canvas.Children.Clear();
            Label background = new Label();
            background.Height = Canvas.Height;
            background.Width = Canvas.Width;
            Canvas.SetZIndex(background, 2);
            background.MouseWheel += GraphicScaleChange;

            Canvas.Children.Add(background);
            Border border = new Border();
            border.Width = Canvas.Width;
            border.Height = Canvas.Height;
            border.BorderThickness = new Thickness(2);
            border.BorderBrush = Brushes.Black;
            Canvas.Children.Add(border);

            drawOX();

            if (Axes.Last() != 0)
                drawOY();


        }

        /// <summary>
        /// Отрисовка оси OX
        /// </summary>
        private void drawOX()
        {
            Line Ox = new Line
            {
                X1 = 0,
                Y1 = centerY,
                X2 = Canvas.Width - 5,
                Y2 = centerY,
                Stroke = Brushes.Black,
                StrokeThickness = 2
            };
            Polygon arrowHeadOy = new Polygon
            {
                Fill = Brushes.Black,
                Points = new PointCollection
            {
                new Point(Ox.X2+5, Ox.Y1),
                new Point(Ox.X2 - 15, Ox.Y1 - 5),
                new Point(Ox.X2 - 15, Ox.Y1 + 5)
            }
            };
            Canvas.Children.Add(Ox);
            Canvas.Children.Add(arrowHeadOy);

            Label x1 = new Label
            {
                FontSize = 25,

            };
            if (Axes != null)
                x1.Content = "x" + Utils.makeLowerIndex(Axes[0] + 1);
            else
                x1.Content = "x" + Utils.makeLowerIndex(1);

            Canvas.Children.Add(x1);
            Canvas.SetTop(x1, centerY - 45);
            Canvas.SetLeft(x1, Canvas.Width - 35);


            for (int i = 0; i != 20; i++)
            {
                if (i != 0 && i != 10)
                {
                    Line sepOx = new Line
                    {
                        X1 = Canvas.Width / 20 * i,
                        X2 = Canvas.Width / 20 * i,
                        Y1 = centerY - 5,
                        Y2 = centerY + 5,
                        Stroke = Brushes.Black,
                        StrokeThickness = 1
                    };
                    Canvas.Children.Add(sepOx);

                    Label sepNum = new Label
                    {
                        FontSize = 12,
                    };
                    if (i < 10)
                    {
                        sepNum.Content = Math.Round(-(10 * Scale - i * Scale), 2);
                        Canvas.SetLeft(sepNum, Canvas.Width / 20 * i - 14);
                    }
                    else
                    {
                        sepNum.Content = Math.Round(i * Scale - 10 * Scale, 2);
                        Canvas.SetLeft(sepNum, Canvas.Width / 20 * i - 9);
                    }
                    Canvas.Children.Add(sepNum);
                    Canvas.SetTop(sepNum, centerY + 15);
                }
                Line vertical = new Line
                {
                    X1 = Canvas.Width / 20 * i,
                    X2 = Canvas.Width / 20 * i,
                    Y1 = 0,
                    Y2 = Canvas.Height,
                    Stroke = Brushes.Black,
                    StrokeThickness = 0.5,
                };
                Canvas.Children.Add(vertical);
                Line horizontal = new Line
                {
                    X1 = 0,
                    X2 = Canvas.Width,
                    Y1 = Canvas.Height / 20 * i,
                    Y2 = Canvas.Height / 20 * i,
                    Stroke = Brushes.Black,
                    StrokeThickness = 0.5,
                };
                Canvas.Children.Add(horizontal);

            }
            if (Axes[1] == 0)
            {
                Line sepOx = new Line
                {
                    X1 = Canvas.Width / 200,
                    X2 = Canvas.Width / 200,
                    Y1 = centerY - 5,
                    Y2 = centerY + 5,
                    Stroke = Brushes.Black,
                    StrokeThickness = 1
                };
                Canvas.Children.Add(sepOx);

                Label sepNum = new Label
                {
                    FontSize = 12,
                };
                sepNum.Content = Math.Round(-(10 * Scale - 10 * Scale), 2);
                Canvas.SetLeft(sepNum, Canvas.Width / 20 * 10 - 9);

                Canvas.Children.Add(sepNum);
                Canvas.SetTop(sepNum, centerY + 15);
            }

        }
        /// <summary>
        /// Отрисовка оси OY
        /// </summary>
        private void drawOY()
        {
            Line Oy = new Line
            {
                X1 = centerX,
                Y1 = 5,
                X2 = centerX,
                Y2 = Canvas.Height,
                Stroke = Brushes.Black,
                StrokeThickness = 2
            };
            Polygon arrowHeadOx = new Polygon
            {
                Fill = Brushes.Black,
                Points = new PointCollection
            {
                new Point(Oy.X1, Oy.Y1-5),
                new Point(Oy.X1 + 5, Oy.Y1 + 15),
                new Point(Oy.X1 - 5, Oy.Y1 + 15)
            }
            };
            Canvas.Children.Add(Oy);
            Canvas.Children.Add(arrowHeadOx);

            Label x2 = new Label
            {
                FontSize = 25,
            };
            if (Axes != null)
                x2.Content = "x" + Utils.makeLowerIndex(Axes[1] + 1);
            else
                x2.Content = "x" + Utils.makeLowerIndex(2);

            Canvas.Children.Add(x2);
            Canvas.SetTop(x2, -5);
            Canvas.SetLeft(x2, centerX + 5);


            for (int i = 0; i != 20; i++)
            {
                if (i != 9 && i != 19)
                {
                    Line sepOy = new Line
                    {
                        X1 = centerX - 5,
                        X2 = centerX + 5,
                        Y1 = Canvas.Height / 20 * (i + 1),
                        Y2 = Canvas.Height / 20 * (i + 1),
                        Stroke = Brushes.Black,
                        StrokeThickness = 1
                    };
                    Canvas.Children.Add(sepOy);

                    if (i == 10)
                        continue;

                    Label sepNum = new Label
                    {
                        FontSize = 12,
                    };
                    if (i < 10)
                    {
                        sepNum.Content = Math.Round(10 * Scale - (i + 1) * Scale, 2);
                        Canvas.SetLeft(sepNum, centerX - 39);
                    }
                    else
                    {
                        sepNum.Content = Math.Round(-(i + 1) * Scale + 10 * Scale, 2);
                        Canvas.SetLeft(sepNum, centerX - 44);
                    }
                    Canvas.SetTop(sepNum, Canvas.Height / 20 * (i + 1) - 15);
                    Canvas.Children.Add(sepNum);
                }

                Line horizontal = new Line
                {
                    X1 = 0,
                    X2 = Canvas.Width,
                    Y1 = Canvas.Height / 20 * i,
                    Y2 = Canvas.Height / 20 * i,
                    Stroke = Brushes.Black,
                    StrokeThickness = 0.5,
                };
                Canvas.Children.Add(horizontal);
            }

        }

        /// <summary>
        /// Отрисовка области допустимых значений которые подходят под все условия<br></br>
        /// Не отрисовывает если область неограничена
        /// </summary>
        public void DrawPolygram()
        {
            if (!isRestricted)
                return;

            Fraction maxY = Fraction.DecimalToFraction(Scale * 10);
            Fraction minY = -maxY;
            Fraction maxX = Fraction.DecimalToFraction(Scale * 10);
            Fraction minX = -maxX;


            points = new List<FractionPoint>
            {
                new FractionPoint(maxX, maxY),
                new FractionPoint(new Fraction(0), maxY),
                new FractionPoint(maxX, new Fraction(0)),
                new FractionPoint(new Fraction(0),new Fraction(0))
            };



            for (int i = 0; i != conditionLines.Count; i++)
            {
                Fraction a = conditionLines[i].a;
                Fraction b = conditionLines[i].b;
                Fraction c = conditionLines[i].c;

                List<FractionPoint> intersections = conditionLines[i].GetIntersections(this);
                foreach (FractionPoint p in intersections)
                    points.Add(p);

                for (int j = i + 1; j != conditionLines.Count; j++)
                {
                    List<Fraction> firstF = conditionLines[i].GetData();
                    List<Fraction> secondF = conditionLines[j].GetData();
                    Matrix matrix = new Matrix(new List<List<Fraction>> { firstF, secondF });
                    Matrix output = Utils.doGauss(matrix, new List<int> { 0, 1 });
                    if (Utils.isLineryIndependent(output, new List<int> { 0, 1 }))
                    {
                        Fraction x = output.Conditions[0][2];
                        Fraction y = output.Conditions[1][2];
                        points.Add(new FractionPoint(x, y));
                    }
                }
            }

            points = points
            .Where(point => conditionLines.All(line =>
                ConditionTypeHelper.CheckCondition(line.conditionType, line.a * point.X + line.b * point.Y, line.c)))
            .ToHashSet().ToList();

            if (Axes[1] == 0)
            {
                points = points.Where(x => Math.Round(x.Y.Value(), 11) != Math.Round(maxY.Value(), 11)).ToList();
            }

            points = Utils.ConvexHull(points);
            if (points.Count >= 2)
            {
                var transformedPoints = points.Select(x => new Point(x.X.Value(), x.Y.Value())).Select(TransformCoordinate).ToList();
                if (points.Count == 2)
                {
                    Line line = new Line
                    {
                        X1 = transformedPoints[0].X,
                        Y1 = transformedPoints[0].Y,
                        X2 = transformedPoints[1].X,
                        Y2 = transformedPoints[1].Y,
                        Stroke = Brushes.Blue,
                        StrokeThickness = 2
                    };

                    Canvas.Children.Add(line);
                }
                else
                {
                    Polygon polygon = new Polygon
                    {
                        Fill = new SolidColorBrush(Color.FromArgb(100, 0, 0, 255)),
                        Stroke = Brushes.Blue,
                        StrokeThickness = 2,
                        Points = new PointCollection(transformedPoints)
                    };

                    Canvas.Children.Add(polygon);
                }

            }
        }
        /// <summary>
        /// Функция отрисовки целевой функции<br></br>
        /// Не отрисовывает если область неограничена
        /// </summary>
        /// <param name="linearFunction">Линейная функция которую нужно отрисовать<br></br>
        /// Если не передана значит уже есть у графика</param>
        /// <returns>Список экстремиумных точек либо null если их нет</returns>
        public List<FractionPoint> DrawMainFunction(LinearFunction linearFunction = null)
        {
            if (!isRestricted)
                return null;
            if (linearFunction != null)
            {
                this.function = linearFunction;
            }
            List<Fraction> koeffs = function.GetRawData();
            Fraction a = koeffs[0];
            Fraction b = koeffs[1];
            Fraction c = koeffs[2];

            Fraction min = null, max = null;
            List<FractionPoint> minPoints = new List<FractionPoint>();
            List<FractionPoint> maxPoints = new List<FractionPoint>();



            for (int i = 0; i < points.Count; i++)
            {
                Fraction value = a * points[i].X + b * points[i].Y + c;
                if (Fraction.decimalFlag)
                    value.decimalValue = Math.Round(value.decimalValue, 10);

                if (min == null || value < min)
                {
                    min = value;
                    minPoints.Clear();
                    minPoints.Add(points[i]);
                }
                else if (value.Equals(min))
                {
                    minPoints.Add(points[i]);
                }

                if (max == null || value > max)
                {
                    max = value;
                    maxPoints.Clear();
                    maxPoints.Add(points[i]);
                }
                else if (value.Equals(max))
                {
                    maxPoints.Add(points[i]);
                }
            }

            FractionPoint extraPoint = null;
            FractionPoint normalPoint = null;

            if (function.min && min != null)
            {
                Fraction sumX = new Fraction(0);
                Fraction sumY = new Fraction(0);

                foreach (var point in minPoints)
                {
                    sumX += point.X;
                    sumY += point.Y;
                }

                extraPoint = new FractionPoint(sumX / new Fraction(minPoints.Count), sumY / new Fraction(minPoints.Count));
                normalPoint = new FractionPoint(-a + extraPoint.X, -b + extraPoint.Y);
            }
            else if (!function.min && max != null)
            {
                Fraction sumX = new Fraction(0);
                Fraction sumY = new Fraction(0);

                foreach (var point in maxPoints)
                {
                    sumX += point.X;
                    sumY += point.Y;
                }

                extraPoint = new FractionPoint(sumX / new Fraction(maxPoints.Count), sumY / new Fraction(maxPoints.Count));
                normalPoint = new FractionPoint(a + extraPoint.X, b + extraPoint.Y);
            }
            if (extraPoint != null && normalPoint != null)
            {
                if (extraPoint == normalPoint)
                    return new List<FractionPoint> { extraPoint };

                c = (a * extraPoint.X + b * extraPoint.Y);
                List<Point> pointsCoordinate = new List<Point>();
                List<FractionPoint> returnablePoints = new List<FractionPoint>();
                if (minPoints.Count == 1 && function.min || maxPoints.Count == 1 && !function.min)
                {
                    returnablePoints.Add(extraPoint);
                    pointsCoordinate = returnablePoints.Select(x => new Point(x.X.Value(), x.Y.Value())).Select(TransformCoordinate).ToList();
                    Ellipse point = new Ellipse()
                    {
                        Fill = Brushes.Purple,
                        Width = 10,
                        Height = 10,
                        Stroke = Brushes.Purple,
                        Margin = new Thickness(pointsCoordinate[0].X - 5, pointsCoordinate[0].Y - 5, 0, 0),
                    };
                    Canvas.SetZIndex(point, 1);
                    Canvas.Children.Add(point);
                }
                else if (function.min)
                {
                    returnablePoints = Utils.FindEdges(minPoints);
                    pointsCoordinate = returnablePoints.Select(x => new Point(x.X.Value(), x.Y.Value())).Select(TransformCoordinate).ToList();
                    foreach (Point point in pointsCoordinate)
                    {
                        Ellipse dot = new Ellipse()
                        {
                            Fill = Brushes.Purple,
                            Width = 10,
                            Height = 10,
                            Stroke = Brushes.Purple,
                            Margin = new Thickness(point.X - 5, point.Y - 5, 0, 0),
                        };
                        Canvas.SetZIndex(dot, 1);
                        Canvas.Children.Add(dot);
                    }
                }
                else
                {
                    returnablePoints = Utils.FindEdges(maxPoints);
                    pointsCoordinate = returnablePoints.Select(x => new Point(x.X.Value(), x.Y.Value())).Select(TransformCoordinate).ToList();
                    foreach (Point point in pointsCoordinate)
                    {
                        Ellipse dot = new Ellipse()
                        {
                            Fill = Brushes.Purple,
                            Width = 10,
                            Height = 10,
                            Stroke = Brushes.Purple,
                            Margin = new Thickness(point.X - 5, point.Y - 5, 0, 0),
                        };
                        Canvas.SetZIndex(dot, 1);
                        Canvas.Children.Add(dot);
                    }
                }


                GraphicLine normalVector = new GraphicLine(extraPoint, normalPoint, Brushes.Orange, 2);
                normalVector.DrawLine(this);
                GraphicLine functionLine = new GraphicLine(a, b, c, new LinearFunction(2, new List<Fraction> { new Fraction(a), new Fraction(b), new Fraction(c) }), Brushes.Red, 2);
                functionLine.DrawLine(this);
                return returnablePoints.ToHashSet().ToList();
            }
            return null;

        }

        /// <summary>
        /// Функция изменения координат из графических в Canvasные точки
        /// </summary>
        /// <param name="initialPoint">Точка которую нужно превратить</param>
        /// <returns>Точка с координатами холста</returns>
        private Point TransformCoordinate(Point initialPoint)
        {
            double scaleCorrection = Canvas.Height / 20 * (1 / Scale);
            return new Point(
                initialPoint.X * scaleCorrection + centerX,
                centerY - initialPoint.Y * scaleCorrection // Инверсия оси Y
            );
        }


    }
}
