using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace LinearTools
{
    public class LinearFunction : LinearData
    {
        /// <summary>
        /// Массив коэффициентов С
        /// </summary>
        public List<TextBox> cList { get; set; }
        /// <summary>
        /// Переменная отвечающая за стремление функции <br></br>
        /// True - к мимимуму, False - к максимуму
        /// </summary>
        public bool min { get; set; }
        /// <summary>
        /// Конструктор функции принимающий xAmount и необязательный параметр cList<br></br>
        /// создающий равенство вида 1 + 2 + 3 +...+xAmount -> min<br></br>
        /// Если передан параметр List<Fraction> cList создает функцию вида<br></br>
        /// cList[0] + ... + cList[xAmount-1] -> min<br></br>
        /// </summary>
        /// <param name="xAmount">Число переменных</param>
        /// <param name="cList">Список Fraction коэффциентов</param>
        public LinearFunction(int xAmount, List<Fraction> cList = null, bool min = true) : base(xAmount)
        {

            this.cList = new List<TextBox>();
            this.min = min;
            if (cList != null && cList.Count == xAmount + 1)
            {
                drawFunction(cList);
            }
            else
            {
                drawFunction();
            }

        }
        ///<summary>
        /// Отрисовка равенства
        /// Если передан параментр cList создает функцию с данными коэффициентами<br></br>
        /// вида cList[0] + ... + cList[xAmount-1] -> min<br></br>
        /// Иначе вида 1 + 2 + 3 +...+xAmount -> min<br></br>
        /// </summary>
        /// <param name="cList">Список коэффициентов</param>
        private void drawFunction(List<Fraction> cList = null)
        {
            Border functionBorder = new Border();
            functionBorder.BorderBrush = Brushes.Black;
            functionBorder.BorderThickness = new Thickness(1);
            functionBorder.Height = 50;
            functionBorder.Width = 275 + (xAmount - 1) * 80;
            Canvas.Children.Add(functionBorder);
            Canvas.Height = 50;

            Label F = new Label();
            F.Content = "F(x)";
            F.Height = 50;
            F.Width = 40;
            F.FontSize = 20;
            F.VerticalContentAlignment = VerticalAlignment.Bottom;
            F.HorizontalContentAlignment = HorizontalAlignment.Left;
            Canvas.Children.Add(F);

            Label equal = new Label();
            equal.Content = "=";
            equal.FontSize = 20;
            equal.Height = 50;
            equal.Width = 25;
            equal.VerticalContentAlignment = VerticalAlignment.Bottom;
            equal.Visibility = Visibility.Visible;
            Canvas.SetLeft(equal, 35);
            Canvas.Children.Add(equal);

            for (int i = 0; i != xAmount + 1; i++)
            {
                TextBox input = new TextBox();
                if (cList != null)
                {
                    input.Text = cList[i].ToString();
                }
                input.FontSize = 16;
                input.Height = 25;
                input.Width = 40;
                input.BorderBrush = Brushes.Black;
                input.VerticalContentAlignment = VerticalAlignment.Bottom;
                input.HorizontalContentAlignment = HorizontalAlignment.Center;
                input.VerticalAlignment = VerticalAlignment.Bottom;
                input.KeyDown += TextBox_KeyDown;
                Canvas.SetTop(input, 20);
                Canvas.SetLeft(input, 60 + (80 * i));
                this.cList.Add(input);
                Canvas.Children.Add(input);

                if (i != xAmount)
                {


                    Label x = new Label();
                    x.Content = "X" + Utils.makeLowerIndex(i + 1);
                    x.FontSize = 16;
                    x.Height = 50;
                    x.Width = 30;
                    x.VerticalContentAlignment = VerticalAlignment.Bottom;
                    x.HorizontalContentAlignment = HorizontalAlignment.Left;
                    Canvas.SetLeft(x, 100 + (80 * i));
                    Canvas.Children.Add(x);
                }


                if (i == xAmount)
                {
                    Label min = new Label();
                    if (this.min)
                    {
                        min.Content = "―> min";
                        min.Foreground = Brushes.Blue;
                        this.min = true;
                    }
                    else
                    {
                        min.Content = "―> max";
                        min.Foreground = Brushes.Red;
                        this.min = false;
                    }
                    min.FontSize = 18;
                    min.Height = 50;
                    min.Width = 85;
                    min.VerticalContentAlignment = VerticalAlignment.Bottom;
                    min.HorizontalContentAlignment = HorizontalAlignment.Left;
                    Canvas.SetLeft(min, 180 + (i - 1) * 80);
                    Canvas.Children.Add(min);
                    min.FontWeight = FontWeights.Bold;

                    min.MouseDown += (sender, e) =>
                    {
                        if (this.min)
                        {
                            min.Content = "―> max";
                            min.Foreground = Brushes.Red;
                            this.min = false;
                        }
                        else
                        {
                            min.Content = "―> min";
                            min.Foreground = Brushes.Blue;
                            this.min = true;
                        }

                    };
                }
                else
                {
                    Label plus = new Label();
                    plus.Content = "+";
                    plus.FontSize = 20;
                    plus.Height = 50;
                    plus.Width = 25;
                    plus.VerticalContentAlignment = VerticalAlignment.Bottom;
                    plus.HorizontalContentAlignment = HorizontalAlignment.Left;
                    Canvas.SetLeft(plus, 120 + 80 * i);
                    Canvas.Children.Add(plus);
                }
            }
        }
        /// <summary>
        /// Возвращает список коэффициентов данной функции<br></br>
        /// И умножает их на минус 1 если функция к максимуму
        /// </summary>
        /// <returns>Список коэффициентов функции к минимуму</returns>
        public List<Fraction> GetData()
        {
            List<Fraction> data = new List<Fraction>();
            for (int i = 0; i <= xAmount; i++)
            {
                if (min)
                    data.Add(Fraction.Parse(cList[i].Text));
                else
                    data.Add(Fraction.Parse(cList[i].Text) * new Fraction(-1));
            }
            return data;
        }
        /// <summary>
        /// Возвращает список коэффициентов без доп множителей
        /// </summary>
        /// <returns>Изначальный список коэффициентов</returns>
        public List<Fraction> GetRawData()
        {
            List<Fraction> data = new List<Fraction>();
            for (int i = 0; i <= xAmount; i++)
            {
                data.Add(Fraction.Parse(cList[i].Text));
            }
            return data;
        }

        public Fraction GetExtremium(List<Fraction> plan)
        {
            Fraction result = new Fraction(0);
            List<Fraction> data = GetData();
            for (int i = 0; i < Math.Min(data.Count, plan.Count); i++)
            {
                result += data[i] * plan[i];
            }
            result += data.Last();
            if (min)
                return result;
            else
                return result * new Fraction(-1);
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                TextBox currentTextBox = sender as TextBox;

                if (currentTextBox == null)
                    return;
                UIElement nextElement = currentTextBox.PredictFocus(FocusNavigationDirection.Right) as UIElement;
                if (nextElement is Button)
                {
                    nextElement = currentTextBox.PredictFocus(FocusNavigationDirection.Down) as UIElement;
                    for (int i = 0; i != xAmount; i++)
                    {
                        nextElement = nextElement.PredictFocus(FocusNavigationDirection.Left) as UIElement;
                    }
                }
                if (nextElement != null && nextElement is TextBox)
                {
                    nextElement.Focus();
                    e.Handled = true;
                }
            }
        }

        /// <summary>
        /// Оператор при обращении к функции в виде UIElement возвращает его холст
        /// </summary>
        public static implicit operator UIElement(LinearFunction function) => function.Canvas;
    }
}
