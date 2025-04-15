using System.Windows;
using System.Windows.Controls;

namespace LinearTools
{
    public abstract class LinearData
    {
        /// <summary>
        /// Холст на котором отрисовывается линейное равенство или функция
        /// </summary>
        public Canvas Canvas { get; set; }
        /// <summary>
        /// Количество переменных
        /// </summary>
        public int xAmount { get; set; }

        public LinearData(int xAmount)
        {
            this.Canvas = new Canvas();
            this.xAmount = xAmount;
        }

        public static implicit operator UIElement(LinearData data) => data.Canvas;

    }
}
