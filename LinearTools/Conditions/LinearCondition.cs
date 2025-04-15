using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace LinearTools
{
    public class LinearCondition : LinearData
    {
        /// <summary>
        /// Список коэффициентов
        /// </summary>
        public List<TextBox> aList { get; private set; }
        /// <summary>
        /// Фон равенства
        /// </summary>
        public Label background { get; private set; }
        /// <summary>
        /// Событие для обработки выделения равенства
        /// </summary>
        public event EventHandler SelectCondition;
        /// <summary>
        /// Событие для обработки удаления равенства
        /// </summary>
        public event EventHandler RemoveCondition;
        public ConditionType conditionType { get; set; }
        /// <summary>
        /// Конструктор равенства принимающий xAmount и необязательный параметр aList
        /// создающий равенство вида 1 + 2 + 3 +...+xAmount = xAmount+1
        /// Если передан параметр List<Fraction> aList создает равенства вида
        /// aList[0] + ... + aList[xAmount-1] = aList[xAmount]
        /// </summary>
        /// <param name="xAmount">Число переменных</param>
        /// <param name="aList">Список Fraction коэффциентов </param>
        public LinearCondition(int xAmount, List<Fraction> aList = null, ConditionType conditionType = ConditionType.Equal) : base(xAmount)
        {
            this.conditionType = conditionType;
            Canvas = new Canvas
            {
                Width = 274 + (xAmount - 2) * 80,
                Height = 50,
            };


            this.aList = new List<TextBox>();

            if (aList != null && aList.Count == xAmount + 1)
            {
                drawCondition(aList);
            }
            else
            {
                drawCondition();
            }
            background = new Label
            {
                Opacity = 0,
                Height = Canvas.Height,
                Width = Canvas.Width,
                Background = Brushes.Red

            };

            Canvas.Children.Add(background);
            background.MouseRightButtonDown += (sender, e) => RemoveCondition?.Invoke(this, EventArgs.Empty);
            background.MouseLeftButtonDown += (sender, e) => SelectCondition?.Invoke(this, EventArgs.Empty);




        }
        /// <summary>
        /// Отрисовка равенства
        /// Если передан параментр aList создает равенство с данными коэффициентами
        /// вида aList[0] + ... + aList[xAmount-1] = aList[xAmount]
        /// Иначе вида 1 + 2 + 3 +...+xAmount = xAmount+1
        /// </summary>
        private void drawCondition(List<Fraction> aList = null)
        {


            Border conditionBorder = new Border();
            conditionBorder.BorderBrush = Brushes.Black;
            conditionBorder.BorderThickness = new Thickness(0, 0, 0, 1);
            conditionBorder.Width = 274 + (xAmount - 2) * 80;
            conditionBorder.Height = 50;

            Canvas.Children.Add(conditionBorder);
            Canvas.Height = 50;

            for (int j = 0; j != xAmount; j++)
            {

                TextBox input = new TextBox();
                if (aList != null)
                {

                    input.Text = aList[j].ToString();
                }
                input.FontSize = 16;
                input.Height = 25;
                input.Width = 40;
                input.BorderBrush = Brushes.Black;
                input.VerticalContentAlignment = VerticalAlignment.Bottom;
                input.HorizontalContentAlignment = HorizontalAlignment.Center;
                input.VerticalAlignment = VerticalAlignment.Bottom;
                input.MaxLength = 23;
                input.KeyDown += TextBox_KeyDown;
                Canvas.SetZIndex(input, 100);
                Canvas.SetTop(input, 20);
                Canvas.SetLeft(input, 5 + (80 * j));

                Canvas.Children.Add(input);
                this.aList.Add(input);

                Label x = new Label();
                x.Content = "X" + Utils.makeLowerIndex(j + 1);
                x.FontSize = 16;
                x.Height = 50;
                x.Width = 30;
                x.VerticalContentAlignment = VerticalAlignment.Bottom;
                Canvas.SetLeft(x, 45 + (80 * j));
                Canvas.Children.Add(x);

                if (j == xAmount - 1)
                {
                    Label equal = new Label();
                    equal.Content = conditionType.GetOperator();
                    equal.FontSize = 25;
                    equal.Height = 50;
                    equal.Width = 30;
                    equal.VerticalContentAlignment = VerticalAlignment.Bottom;
                    equal.Uid = "equal";
                    Canvas.SetZIndex(equal, 1);
                    equal.Foreground = conditionType.GetColor();
                    equal.FontWeight = FontWeights.Bold;
                    equal.MouseLeftButtonDown += (sender, e) =>
                    {
                        conditionType = conditionType.NextCondition();
                        equal.Content = conditionType.GetOperator();
                        equal.Foreground = conditionType.GetColor();
                    };
                    equal.MouseRightButtonDown += (sender, e) =>
                    {
                        conditionType = conditionType.PreviousCondition();
                        equal.Content = conditionType.GetOperator();
                        equal.Foreground = conditionType.GetColor();
                    };
                    Canvas.SetLeft(equal, 145 + (j - 1) * 80);
                    Canvas.Children.Add(equal);

                    TextBox B = new TextBox();
                    if (aList != null)
                    {
                        B.Text = aList[j + 1].ToString();
                    }
                    B.FontSize = 16;
                    B.Height = 25;
                    B.Width = 40;
                    B.BorderBrush = Brushes.Black;
                    B.VerticalContentAlignment = VerticalAlignment.Bottom;
                    B.HorizontalContentAlignment = HorizontalAlignment.Center;
                    B.VerticalAlignment = VerticalAlignment.Bottom;
                    B.KeyDown += TextBox_KeyDown;
                    Canvas.SetTop(B, 20);
                    Canvas.SetLeft(B, 170 + (j - 1) * 80);
                    Canvas.SetZIndex(B, 100);
                    Canvas.Children.Add(B);
                    this.aList.Add(B);

                }
                else
                {
                    Label plus = new Label();
                    plus.Content = "+";
                    plus.FontSize = 20;
                    plus.Height = 50;
                    plus.Width = 25;
                    plus.VerticalContentAlignment = VerticalAlignment.Bottom;
                    Canvas.SetLeft(plus, 65 + (80 * j));
                    Canvas.Children.Add(plus);
                }


            }
        }
        /// <summary>
        /// Возвращает список коэффициентов данного равенства
        /// </summary>
        /// <returns></returns>
        public List<Fraction> getData()
        {
            List<Fraction> data = new List<Fraction>();
            for (int i = 0; i < xAmount + 1; i++)
            {
                data.Add(Fraction.Parse(aList[i].Text));
            }
            return data;
        }
        /// <summary>
        /// Оператор при обращении к равенству в виде UIElement возвращает его холст
        /// </summary>
        public static implicit operator UIElement(LinearCondition condition) => condition.Canvas;

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            TextBox currentTextBox = sender as TextBox;
            if (currentTextBox == null)
                return;
            if (e.Key == Key.Enter)
            {
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

    }
}
