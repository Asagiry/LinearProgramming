using System;
using System.Windows.Media;

namespace LinearTools
{

    public enum ConditionType
    {
        Equal = 0,
        LessOrEqualThen = 1,
        MoreOrEqualThen = 2,

    }
    public static class ConditionTypeHelper
    {
        public static string GetOperator(this ConditionType condition)
        {
            switch (condition)
            {
                case ConditionType.Equal:
                    return "=";
                case ConditionType.LessOrEqualThen:
                    return "⩽";
                case ConditionType.MoreOrEqualThen:
                    return "⩾";
                default:
                    throw new ArgumentOutOfRangeException(nameof(condition), condition, null);
            }
        }
        public static Brush GetColor(this ConditionType condition)
        {
            switch (condition)
            {
                case ConditionType.Equal:
                    return Brushes.LightCoral;
                case ConditionType.LessOrEqualThen:
                    return Brushes.Red;
                case ConditionType.MoreOrEqualThen:
                    return Brushes.DodgerBlue;
                default:
                    throw new ArgumentOutOfRangeException(nameof(condition), condition, null);
            }
        }
        public static ConditionType NextCondition(this ConditionType condition)
        {
            if (condition == ConditionType.MoreOrEqualThen)
                return ConditionType.Equal;
            else
                return condition + 1;
        }
        public static ConditionType PreviousCondition(this ConditionType condition)
        {
            if (condition == ConditionType.Equal)
                return ConditionType.MoreOrEqualThen;
            else
                return condition - 1;
        }
        public static bool CheckCondition(ConditionType condition, Fraction value1, Fraction value2)
        {
            if (Fraction.decimalFlag)
            {
                value1 = new Fraction(Math.Round(value1.Value(), 11));
                value2 = new Fraction(Math.Round(value2.Value(), 11));
            }
            switch (condition)
            {
                case ConditionType.Equal:
                    return value1.isEqualTo(value2);
                case ConditionType.LessOrEqualThen:
                    return value1 <= value2; ;
                case ConditionType.MoreOrEqualThen:
                    return value1 >= value2;
                default:
                    throw new ArgumentOutOfRangeException(nameof(condition), condition, null);
            }
        }

    }
}
