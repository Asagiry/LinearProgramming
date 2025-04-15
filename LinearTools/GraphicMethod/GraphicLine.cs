using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace LinearTools.GraphicMethod
{

    public class GraphicLine
    {
        /// <summary>
        /// Линия графика
        /// </summary>
        public Line line { get; set; }
        /// <summary>
        /// Первая точка линии графика
        /// </summary>
        public Point startPoint { get; set; }
        /// <summary>
        /// Вторая точка линии графика
        /// </summary>
        public Point endPoint { get; set; }
        /// <summary>
        /// График на котором отрисована линия
        /// </summary>
        public Graphic graphic { get; set; }
        /// <summary>
        /// Коэффциент а
        /// </summary>
        public Fraction a { get; set; }
        /// <summary>
        /// Коэффициент б
        /// </summary>
        public Fraction b { get; set; }
        /// <summary>
        /// Свободный член
        /// </summary>
        public Fraction c { get; set; }
        /// <summary>
        /// Линейная информация о линии
        /// </summary>
        public LinearData linearCondition { get; set; }
        /// <summary>
        /// Тип условия, больше равно, меньше равно, равно
        /// </summary>
        public ConditionType conditionType { get; set; }
        /// <summary>
        /// Цвет линии
        /// </summary>
        public SolidColorBrush color { get; set; }
        /// <summary>
        /// Толщина линии
        /// </summary>
        public double thickness { get; set; }
        public FrameworkElement area { get; set; }

        /// <summary>
        /// Конструктор линии для линейного ограничения 
        /// </summary>
        /// <param name="linearCondition">Линейное ограничение</param>
        /// <param name="color">Цвет</param>
        /// <param name="thickness">Толщина</param>
        /// <exception cref="Exception">Если переменных больше 2</exception>
        public GraphicLine(LinearCondition linearCondition, SolidColorBrush color = null, double thickness = 1)
        {
            if (linearCondition.xAmount != 2)
            {
                throw new Exception("Количество переменных не равняется 2, получено " + linearCondition.xAmount);
            }

            this.linearCondition = linearCondition;
            this.conditionType = linearCondition.conditionType;

            List<Fraction> fractions = linearCondition.getData();
            this.a = fractions[0];
            this.b = fractions[1];
            this.c = fractions[2];

            this.color = color ?? Brushes.Red;
            this.thickness = thickness;

        }
        /// <summary>
        /// Конструктор для ограничений X1>0, X2>0 на графике
        /// </summary>
        /// <param name="x"></param>
        public GraphicLine(int x)
        {
            if (x == 1 || x == 2)
            {
                Fraction aCoeff = new Fraction(x == 1 ? 1 : 0);
                Fraction bCoeff = new Fraction(x == 1 ? 0 : 1);

                List<Fraction> coeffs = new List<Fraction> { aCoeff, bCoeff, new Fraction(0) };
                LinearCondition linearCondition = new LinearCondition(2, coeffs, ConditionType.MoreOrEqualThen);

                this.linearCondition = linearCondition;
                this.conditionType = linearCondition.conditionType;
                this.a = aCoeff;
                this.b = bCoeff;
                this.c = new Fraction(0);
                this.color = Brushes.Transparent;
                this.thickness = 0;

            }
        }
        /// <summary>
        /// Конструктор для линии уровня
        /// </summary>
        /// <param name="a">Коэффициент а</param>
        /// <param name="b">Коэффициент б</param>
        /// <param name="c">Коэффициент с</param>
        /// <param name="linearFunction">Линейная фунция</param>
        /// <param name="color">Цвет линии уровня</param>
        /// <param name="thickness">Толщина линии уровня</param>
        /// <exception cref="Exception">Если количество переменных больше 2 в линейной функции</exception>
        public GraphicLine(Fraction a, Fraction b, Fraction c, LinearFunction linearFunction, SolidColorBrush color = null, double thickness = 2)
        {
            if (linearFunction.xAmount != 2)
            {
                throw new Exception("Количество переменных не равняется 2, получено " + linearFunction.xAmount);
            }
            this.linearCondition = linearFunction;
            this.conditionType = ConditionType.Equal;
            this.a = a;
            this.b = b;
            this.c = c;

            this.color = color ?? Brushes.Red;
            this.thickness = thickness;
        }

        /// <summary>
        /// Конструктор для линии нормали
        /// </summary>
        /// <param name="firstPoint">Начальная точка</param>
        /// <param name="secondPoint">Конечная точка</param>
        /// <param name="color">Цвет нормали</param>
        /// <param name="thickness">Толщина линии нормали</param>
        public GraphicLine(FractionPoint firstPoint, FractionPoint secondPoint, SolidColorBrush color = null, double thickness = 2)
        {
            this.a = secondPoint.Y - firstPoint.Y;
            this.b = firstPoint.X - secondPoint.X;
            this.c = -(secondPoint.X * firstPoint.Y - firstPoint.X * secondPoint.Y);
            this.linearCondition = null;
            this.conditionType = ConditionType.Equal;
            this.color = color ?? Brushes.Red;
            this.thickness = thickness;
            this.startPoint = firstPoint;
            this.endPoint = secondPoint;
        }

        /// <summary>
        /// Отрисовать линию на графике
        /// </summary>
        /// <param name="graphic"></param>
        public void DrawLine(Graphic graphic)
        {
            this.graphic = graphic;

            if (this.linearCondition == null)
            {
                DrawNormalLine();
                return;
            }

            if (a == 0 || b == 0)
            {
                if (a == 0 && b == 0)
                    return;
                if (a == 0)
                    DrawHorizontalLine(graphic);
                else
                    DrawVerticalLine(graphic);
                return;
            }

            List<FractionPoint> intersections = GetIntersections(graphic);

            if (intersections.Count >= 2)
            {
                Point first = TransformCoordinate(intersections[0]);
                Point second = TransformCoordinate(intersections[1]);

                Line func = new Line
                {
                    X1 = first.X,
                    X2 = second.X,
                    Y1 = first.Y,
                    Y2 = second.Y,
                    Stroke = color,
                    StrokeThickness = thickness,
                };
                graphic.Canvas.Children.Add(func);
                this.line = line;
                this.startPoint = first;
                this.endPoint = second;
            }
        }

        /// <summary>
        /// Отрисовка вертикальной линии
        /// </summary>
        /// <param name="graphic"></param>
        private void DrawVerticalLine(Graphic graphic)
        {
            Point coordinate = TransformCoordinate(new Point(c.Value() / a.Value(), 0));


            // Создание линии
            Line line = new Line
            {
                X1 = coordinate.X,
                X2 = coordinate.X,
                Y1 = 0,
                Y2 = graphic.Canvas.Height,
                StrokeThickness = thickness,
                Stroke = (a == 1 && b == 0 && c == 0 && conditionType == ConditionType.MoreOrEqualThen)
                    ? Brushes.Transparent
                    : color ?? Brushes.Red
            };

            // Добавление линии на Canvas
            graphic.Canvas.Children.Add(line);

            this.line = line;
            this.startPoint = new Point(coordinate.X, 0);
            this.endPoint = new Point(coordinate.X, graphic.Canvas.Height);
        }

        /// <summary>
        /// Отрисовка горизонтальной линии
        /// </summary>
        /// <param name="graphic"></param>
        private void DrawHorizontalLine(Graphic graphic)
        {
            Point coordinate = TransformCoordinate(new Point(0, c.Value() / b.Value()));

            if (Math.Abs(coordinate.Y) > graphic.Canvas.Height)
                return;

            Line line = new Line
            {
                X1 = 0,
                X2 = graphic.Canvas.Width,
                Y1 = coordinate.Y,
                Y2 = coordinate.Y,
                StrokeThickness = thickness,
                Stroke = (a == 0 && b == 1 && c == 0 && conditionType == ConditionType.MoreOrEqualThen)
                    ? Brushes.Transparent
                    : color ?? Brushes.Red
            };

            graphic.Canvas.Children.Add(line);
            this.line = line;
            this.startPoint = new Point(0, coordinate.Y);
            this.endPoint = new Point(graphic.Canvas.Width, coordinate.Y);

        }

        /// <summary>
        /// Отрисовка линии нормали
        /// </summary>
        public void DrawNormalLine()
        {
            Point firstPoint = TransformCoordinate(startPoint);
            Point secondPoint = TransformCoordinate(endPoint);

            double dx = endPoint.X - startPoint.X;
            double dy = endPoint.Y - startPoint.Y;

            double dist = Utils.FindDistance(startPoint, endPoint);
            if (dist != 0)
            {
                double scale = 3 / dist;
                secondPoint = TransformCoordinate(new Point(
                    startPoint.X + dx * scale,
                    startPoint.Y + dy * scale
                ));
            }
            else
            {
                secondPoint = TransformCoordinate(new Point(
                    startPoint.X + 3,
                    startPoint.Y
                ));
            }

            Line func = new Line
            {
                X1 = firstPoint.X,
                X2 = secondPoint.X,
                Y1 = firstPoint.Y,
                Y2 = secondPoint.Y,
                Stroke = color,
                StrokeThickness = thickness,
            };

            double angle = Math.Atan2(func.Y2 - func.Y1, func.X2 - func.X1);
            double arrowSize = 10;
            double angleOffset = Math.PI / 6;
            Point arrowTip = new Point(func.X2, func.Y2);
            Point arrowLeft = new Point(
                arrowTip.X - arrowSize * Math.Cos(angle - angleOffset),
                arrowTip.Y - arrowSize * Math.Sin(angle - angleOffset)
            );
            Point arrowRight = new Point(
                arrowTip.X - arrowSize * Math.Cos(angle + angleOffset),
                arrowTip.Y - arrowSize * Math.Sin(angle + angleOffset)
            );
            Polyline arrow = new Polyline
            {
                Stroke = color,
                StrokeThickness = thickness,
                Points = new PointCollection { arrowLeft, arrowTip, arrowRight }
            };
            graphic.Canvas.Children.Add(func);
            graphic.Canvas.Children.Add(arrow);
        }

        /// <summary>
        /// Функция изменения координат из графических в Canvasные точки
        /// </summary>
        /// <param name="initialPoint">Точка которую нужно превратить</param>
        /// <returns>Точка с координатами холста</returns>
        private Point TransformCoordinate(Point initialPoint)
        {
            double scaleCorrection = graphic.Canvas.Height / 20 * (1 / graphic.Scale);
            return new Point(
                initialPoint.X * scaleCorrection + graphic.centerX,
                graphic.centerY - initialPoint.Y * scaleCorrection
            );
        }

        /// <summary>
        /// Функция получения коэффициентов из линии
        /// </summary>
        /// <returns></returns>
        public List<Fraction> GetData()
        {
            if (linearCondition is LinearCondition condition)
                return condition.getData();
            else if (linearCondition is LinearFunction funct)
                return funct.GetRawData();
            return null;
        }

        /// <summary>
        /// Функция получения точек пересечения функции с границами переданного графика
        /// </summary>
        /// <param name="graphic">График где нужно найти точки на границах</param>
        /// <returns>Список точек</returns>
        public List<FractionPoint> GetIntersections(Graphic graphic)
        {
            Fraction maxY = Fraction.DecimalToFraction(graphic.Scale * 10);
            Fraction minY = -maxY;
            Fraction maxX = Fraction.DecimalToFraction(graphic.Scale * 10);
            Fraction minX = -maxX;

            List<FractionPoint> intersections = new List<FractionPoint>();

            foreach (Fraction x in new[] { maxX, minX })
            {
                if (b == 0)
                    break;
                Fraction y = (c - a * x) / b;
                if (y <= maxY && y >= minY)
                    intersections.Add(new FractionPoint(x, y));
            }

            foreach (Fraction y in new[] { maxY, minY })
            {
                if (a == 0)
                    break;
                Fraction x = (c - b * y) / a;
                if (x <= maxX && x >= minX)
                    intersections.Add(new FractionPoint(x, y));
            }
            return intersections.ToHashSet().ToList();
        }

        /// <summary>
        /// Отрисовка области значений функции
        /// </summary>
        public void DrawArea()
        {
            if (area != null && graphic.Canvas.Children.Contains(area))
                return;
            List<FractionPoint> intersections = GetIntersections(graphic);
            if (intersections.Count == 0)
            {
                return;
            }

            Fraction maxY = Fraction.DecimalToFraction(graphic.Scale * 10);
            Fraction minY = -maxY;
            Fraction maxX = Fraction.DecimalToFraction(graphic.Scale * 10);
            Fraction minX = -maxX;
            if (graphic.Axes[1] == 0)
            {
                intersections = intersections.Select(x => new FractionPoint(x.X, new Fraction(0))).ToList();

                intersections.Add(new FractionPoint(minX, new Fraction(0)));

                intersections.Add(new FractionPoint(maxX, new Fraction(0)));
            }
            else
            {
                intersections.Add(new FractionPoint(maxX, maxY));

                intersections.Add(new FractionPoint(maxX, minY));

                intersections.Add(new FractionPoint(minX, maxY));

                intersections.Add(new FractionPoint(minX, minY));
            }
            intersections = intersections.Where(x => ConditionTypeHelper.CheckCondition(conditionType, a * x.X + b * x.Y, c)).ToList();

            intersections = Utils.ConvexHull(intersections);
            List<Point> polygonPoints = intersections.Select(p => new Point(p.X.Value(), p.Y.Value())).Select(TransformCoordinate).ToList();

            if (polygonPoints.Count == 2)
            {
                Line polygon = new Line
                {
                    X1 = polygonPoints[0].X,
                    Y1 = polygonPoints[0].Y,
                    X2 = polygonPoints[1].X,
                    Y2 = polygonPoints[1].Y,
                    Stroke = color,
                    StrokeThickness = 2,
                };

                polygon.Uid = "polygon";
                graphic.Canvas.Children.Add(polygon);
                area = polygon;
            }
            else
            {
                Polygon polygon = new Polygon
                {
                    Fill = new SolidColorBrush(Color.FromArgb(100, color.Color.R, color.Color.G, color.Color.B)),
                    Stroke = color,
                    StrokeThickness = 2,
                    Points = new PointCollection(polygonPoints)
                };
                polygon.Uid = "polygon";
                graphic.Canvas.Children.Add(polygon);
                area = polygon;
            }



        }
        /// <summary>
        /// Удаляет область значений если она есть
        /// </summary>
        public void RemoveArea()
        {
            if (area == null)
                return;
            graphic.Canvas.Children.Remove(area);
            area = null;
        }
    }
}
