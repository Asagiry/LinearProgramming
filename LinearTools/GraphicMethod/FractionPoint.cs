using System.Windows;

namespace LinearTools
{
    public class FractionPoint
    {
        public Fraction X { get; set; }
        public Fraction Y { get; set; }
        public FractionPoint(Fraction x, Fraction y)
        {
            X = x;
            Y = y;
        }
        public FractionPoint()
        {
            X = new Fraction(0);
            Y = new Fraction(0);
        }

        public Point ToPoint()
        {
            return new Point(X.Value(), Y.Value());
        }

        public static implicit operator Point(FractionPoint fractionPoint)
        {
            return new Point(fractionPoint.X.Value(), fractionPoint.Y.Value());
        }
        public override bool Equals(object obj)
        {
            if (obj is FractionPoint other)
            {
                return X.Equals(other.X) && Y.Equals(other.Y);
            }
            return false;
        }
        public override int GetHashCode()
        {
            return X.GetHashCode() ^ Y.GetHashCode();
        }

    }
}
