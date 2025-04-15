using System;
using System.Numerics;

namespace LinearTools
{
    public class Fraction
    {
        public static bool decimalFlag { get; set; }

        public BigInteger numerator { get; set; }
        public BigInteger denominator { get; set; }

        public double decimalValue { get; set; }

        public Fraction(BigInteger numerator, BigInteger denominator)
        {
            if (denominator < 0)
            {
                numerator = -numerator;
                denominator = -denominator;
            }
            this.numerator = numerator;
            this.denominator = denominator;
        }
        public Fraction(double decimalValue)
        {
            if (decimalFlag)
                this.decimalValue = decimalValue;
            else
            {
                this.numerator = (int)decimalValue;
                this.denominator = 1;
            }
        }
        public Fraction(long value)
        {
            if (decimalFlag)
                this.decimalValue = value;
            else
            {
                this.numerator = value;
                this.denominator = 1;
            }
        }
        public Fraction(Fraction fraction)
        {
            if (decimalFlag)
                if (fraction == null)
                    this.decimalValue = double.NaN;
                else
                {
                    this.decimalValue = fraction.decimalValue;
                }
            else
            {
                this.numerator = fraction.numerator;
                this.denominator = fraction.denominator;
            }

        }

        public static implicit operator string(Fraction fraction)
        {
            if (decimalFlag)
                return Math.Round(fraction.decimalValue, 11).ToString();

            if (fraction.numerator % fraction.denominator == 0)
            {
                return (fraction.numerator / fraction.denominator).ToString();
            }
            return fraction.numerator.ToString() + "/" + fraction.denominator.ToString();
        }
        public override string ToString()
        {
            if (decimalFlag)
                return Math.Round(decimalValue, 11).ToString();

            if (numerator % denominator == 0)
            {
                return (numerator / denominator).ToString();
            }
            return numerator.ToString() + "/" + denominator.ToString();
        }

        public static Fraction operator +(Fraction a, Fraction b)
        {
            if (decimalFlag)
            {
                if (b == null || a == null)
                {
                    return a ?? b;
                }
                if (Math.Abs(a.decimalValue + b.decimalValue) <= 0.0000000000001 && Math.Abs(a.decimalValue + b.decimalValue) >= 0)
                    return new Fraction(0);
                return new Fraction(a.decimalValue + b.decimalValue);
            }

            BigInteger commonDenominator = a.denominator * b.denominator;
            BigInteger newNumerator = a.numerator * b.denominator + b.numerator * a.denominator;

            return Simplify(newNumerator, commonDenominator);
        }
        // Оператор вычитания
        public static Fraction operator -(Fraction a, Fraction b)
        {
            if (decimalFlag)
            {
                if (b == null || a == null)
                {
                    return a ?? b;
                }
                if (Math.Abs(a.decimalValue - b.decimalValue) <= 0.0000000000001 && Math.Abs(a.decimalValue - b.decimalValue) >= 0)
                    return new Fraction(0);
                return new Fraction(a.decimalValue - b.decimalValue);
            }

            BigInteger commonDenominator = a.denominator * b.denominator;
            BigInteger newNumerator = a.numerator * b.denominator - b.numerator * a.denominator;

            return Simplify(newNumerator, commonDenominator);
        }
        public static Fraction operator *(Fraction a, Fraction b)
        {
            if (decimalFlag)
            {
                if (b == null || a == null)
                {
                    return a ?? b;
                }
                if (Math.Abs(a.decimalValue * b.decimalValue) <= 0.0000000000001 && Math.Abs(a.decimalValue * b.decimalValue) >= 0)
                    return new Fraction(0);
                return new Fraction(a.decimalValue * b.decimalValue);
            }
            BigInteger newNumerator = a.numerator * b.numerator;
            BigInteger newDenominator = a.denominator * b.denominator;

            return Simplify(newNumerator, newDenominator);
        }
        public static Fraction operator /(Fraction a, Fraction b)
        {
            if (decimalFlag)
            {
                if (b.decimalValue == 0)
                    return null;
                if (Math.Abs(a.decimalValue / b.decimalValue) <= 0.0000000000001 && Math.Abs(a.decimalValue / b.decimalValue) >= 0)
                    return new Fraction(0);
                return new Fraction(a.decimalValue / b.decimalValue);
            }
            if (b.numerator == 0)
                return null;

            BigInteger newNumerator = a.numerator * b.denominator;
            BigInteger newDenominator = a.denominator * b.numerator;

            return Simplify(newNumerator, newDenominator);
        }
        public static Fraction Simplify(BigInteger numerator, BigInteger denominator)
        {
            BigInteger gcd = GCD(numerator, denominator);


            Fraction result = new Fraction(numerator / gcd, denominator / gcd);

            return result;
        }
        private static BigInteger GCD(BigInteger a, BigInteger b)
        {
            while (b != 0)
            {
                BigInteger temp = b;
                b = a % b;
                a = temp;
            }
            return BigInteger.Abs(a);
        }
        public static Fraction Parse(string s)
        {
            // Убираем лишние пробелы
            s = s.Trim();

            // Проверка, содержит ли строка символ "/"
            if (s.Contains("/"))
            {
                // Разделяем строку на части по символу "/"
                string[] parts = s.Split('/');

                // Проверяем, что в строке есть ровно две части (числитель и знаменатель)
                if (parts.Length != 2)
                    throw new FormatException("Некорректный формат строки. Ожидался формат 'числитель/знаменатель'. Получено = " + s);

                // Пробуем преобразовать части строки в целые числа
                if (!BigInteger.TryParse(parts[0], out BigInteger numerator))
                    throw new FormatException("Числитель имеет неверный формат. Получено = " + s);

                if (!BigInteger.TryParse(parts[1], out BigInteger denominator))
                    throw new FormatException("Знаменатель имеет неверный формат. Получено = " + s);

                // Проверка на ноль в знаменателе
                if (denominator == 0)
                    throw new DivideByZeroException("Знаменатель не может быть равен нулю. Получено = " + s);

                Fraction result = new Fraction(numerator, denominator);
                result.decimalValue = (double)numerator / (double)denominator;
                return result;

            }
            else if (s.Contains(","))
            {

                if (!double.TryParse(s, out double doubler))
                    throw new FormatException("Знаменатель имеет неверный формат. Получено = " + s);

                string valueStr = doubler.ToString();
                long decimalPointIndex = valueStr.IndexOf(',');
                if (decimalPointIndex == -1)
                {
                    decimalPointIndex = valueStr.IndexOf('.');
                }
                long decimalPlaces = valueStr.Length - decimalPointIndex - 1;

                BigInteger denum = (BigInteger)Math.Pow(10, decimalPlaces);
                BigInteger num = (BigInteger)(doubler * (long)denum);
                BigInteger gcd = GCD(num, denum);

                Fraction result = new Fraction(num, denum);
                result.decimalValue = doubler;
                return result;
            } 
            else
            {

                if (!BigInteger.TryParse(s, out BigInteger numerator))
                    throw new FormatException("Некорректный формат строки. Ожидалось целое число или дробь. Получено = " + s);
                // Возвращаем дробь, где знаменатель равен 1

                Fraction result = new Fraction(numerator, 1);
                result.decimalValue = (double)numerator;
                return result;

            }
        }

        public static bool operator ==(Fraction a, long b)
        {
            if (decimalFlag)
            {
                return a.decimalValue == b;
            }
            // Приводим целое число b к дроби b/1 и сравниваем с дробью a
            return a.numerator == b * a.denominator;
        }
        public static bool operator !=(Fraction a, long b)
        {
            if (decimalFlag)
            {
                return a.decimalValue != b;
            }
            // Приводим целое число b к дроби b/1 и проверяем на неравенство
            return !(a == b);
        }
        public static bool operator <(Fraction a, long b)
        {
            if (decimalFlag)
            {
                return a.decimalValue < b;
            }

            return a.numerator < (b * a.denominator);
        }

        public static bool operator <(long b, Fraction a)
        {
            if (decimalFlag)
            {
                return b < a.decimalValue;
            }
            // Приводим целое число b к дроби b/1 и сравниваем с дробью a
            return b * a.denominator < a.numerator;
        }
        public static bool operator >(Fraction a, long b)
        {
            if (decimalFlag)
            {
                return a.decimalValue > b;
            }
            return a.numerator > b * a.denominator;
        }

        public static bool operator >(long b, Fraction a)
        {
            if (decimalFlag)
            {
                return b > a.decimalValue;
            }
            return b * a.denominator > a.numerator;
        }
        public static Fraction swap(Fraction a)
        {
            if (decimalFlag)
                return new Fraction(1.0 / a.decimalValue);
            return new Fraction(a.denominator, a.numerator);
        }

        public override bool Equals(object obj)
        {
            return obj is Fraction fraction &&
                   numerator.Equals(fraction.numerator) &&
                   denominator.Equals(fraction.denominator) &&
                   decimalValue == fraction.decimalValue;
        }

        public override int GetHashCode()
        {
            int hashCode = -477951276;
            hashCode = hashCode * -1521134295 + numerator.GetHashCode();
            hashCode = hashCode * -1521134295 + denominator.GetHashCode();
            hashCode = hashCode * -1521134295 + decimalValue.GetHashCode();
            return hashCode;
        }

        public static bool operator <=(Fraction a, long b)
        {
            if (decimalFlag)
            {
                return a.decimalValue <= b;
            }
            // Приводим целое число b к дроби b/1 и сравниваем с дробью a
            return a.numerator <= b * a.denominator;
        }

        public static bool operator <=(long b, Fraction a)
        {
            if (decimalFlag)
            {
                return b <= a.decimalValue;
            }
            // Приводим целое число b к дроби b/1 и сравниваем с дробью a
            return b * a.denominator <= a.numerator;
        }
        public static bool operator >=(Fraction a, long b)
        {
            if (decimalFlag)
            {
                return a.decimalValue >= b;
            }
            return a.numerator >= b * a.denominator;
        }

        public static bool operator >=(long b, Fraction a)
        {
            if (decimalFlag)
            {
                return b >= a.decimalValue;
            }
            return b * a.denominator >= a.numerator;
        }
        public static bool operator <(Fraction a, Fraction b)
        {
            if (b == null)
                return false;
            if (decimalFlag)
            {
                return a.decimalValue < b.decimalValue;
            }
            return a.numerator * b.denominator < b.numerator * a.denominator;
        }

        public static bool operator >(Fraction a, Fraction b)
        {
            if (b == null)
                return true;
            if (decimalFlag)
            {
                return a.decimalValue > b.decimalValue;
            }
            return a.numerator * b.denominator > b.numerator * a.denominator;
        }
        public static bool operator <=(Fraction a, Fraction b)
        {
            if (b == null)
                return false;
            if (decimalFlag)
            {
                return a.decimalValue <= b.decimalValue;
            }
            return a.numerator * b.denominator <= b.numerator * a.denominator;
        }
        public static bool operator >=(Fraction a, Fraction b)
        {
            if (b == null)
                return true;
            if (decimalFlag)
            {
                return a.decimalValue >= b.decimalValue;
            }
            return a.numerator * b.denominator >= b.numerator * a.denominator;
        }
        public bool isEqualTo(Fraction b)
        {
            if (b == null)
                return false;

            if (decimalFlag)
                return decimalValue == b.decimalValue;

            return numerator * b.denominator == b.numerator * denominator;
        }
        public double Value()
        {
            if (decimalFlag)
                return decimalValue;
            else
                return (double)numerator / (double)denominator;
        }
        public static Fraction DecimalToFraction(double decimalValue, int maxDenominator = 1000)
        {
            if (decimalFlag)
                return new Fraction(decimalValue);
            if (Math.Abs(decimalValue - Math.Round(decimalValue)) < 1.0E-10)
            {
                return new Fraction((BigInteger)decimalValue, 1); // Целое число
            }

            int sign = decimalValue < 0 ? -1 : 1; // Учитываем знак числа
            decimalValue = Math.Abs(decimalValue);

            // Алгоритм продолженных дробей для нахождения числителя и знаменателя
            long numerator = 0, denominator = 1, prevNumerator = 1, prevDenominator = 0;
            double fraction = decimalValue;

            while (true)
            {
                // Получаем целую часть
                long integerPart = (long)Math.Floor(fraction);
                long tempNumerator = numerator, tempDenominator = denominator;

                // Обновляем числитель и знаменатель
                numerator = integerPart * numerator + prevNumerator;
                denominator = integerPart * denominator + prevDenominator;

                prevNumerator = tempNumerator;
                prevDenominator = tempDenominator;

                // Проверяем ограничение на знаменатель
                if (denominator > maxDenominator)
                {
                    numerator = prevNumerator;
                    denominator = prevDenominator;
                    break;
                }

                // Проверяем, достигли ли мы нужной точности
                if (Math.Abs((double)numerator / denominator - decimalValue) < 1.0E-10)
                    break;

                // Переходим к следующей дроби
                fraction = 1 / (fraction - integerPart);
            }

            return Simplify(sign * numerator, denominator);
        }

        // Функция для упрощения дроби
        public static Fraction operator -(Fraction fraction)
        {
            return new Fraction(-1) * fraction;
        }
        public Fraction Pow(int exponent)
        {
            if (exponent == 0)
                return new Fraction(1, 1); // Любое число в степени 0 равно 1

            BigInteger newNumerator = BigInteger.Pow(this.numerator, Math.Abs(exponent));
            BigInteger newDenominator = BigInteger.Pow(this.denominator, Math.Abs(exponent));

            if (exponent < 0) // Для отрицательной степени
                return new Fraction(newDenominator, newNumerator);

            return new Fraction(newNumerator, newDenominator);
        }


    }
}