using System;

namespace IdleSoccerClubMVP.Core.Economy
{
    [Serializable]
    public readonly struct EconomyValue : IEquatable<EconomyValue>, IComparable<EconomyValue>
    {
        public EconomyValue(long rawValue)
        {
            RawValue = rawValue;
        }

        public long RawValue { get; }

        public static EconomyValue Zero
        {
            get { return new EconomyValue(0L); }
        }

        public int CompareTo(EconomyValue other)
        {
            return RawValue.CompareTo(other.RawValue);
        }

        public bool Equals(EconomyValue other)
        {
            return RawValue == other.RawValue;
        }

        public override bool Equals(object obj)
        {
            return obj is EconomyValue other && Equals(other);
        }

        public override int GetHashCode()
        {
            return RawValue.GetHashCode();
        }

        public override string ToString()
        {
            return ToWholeString();
        }

        public string ToWholeString()
        {
            return NumberNotationFormatter.FormatWhole(RawValue);
        }

        public string ToCompactString()
        {
            return NumberNotationFormatter.FormatCompact(RawValue);
        }

        public string ToUiString(bool compactByDefault = true)
        {
            return compactByDefault
                ? NumberNotationFormatter.FormatForUi(RawValue)
                : NumberNotationFormatter.FormatWhole(RawValue);
        }

        public static EconomyValue FromInt(int value)
        {
            return new EconomyValue(value);
        }

        public static EconomyValue FromLong(long value)
        {
            return new EconomyValue(value);
        }

        public static EconomyValue operator +(EconomyValue left, EconomyValue right)
        {
            return new EconomyValue(left.RawValue + right.RawValue);
        }

        public static EconomyValue operator -(EconomyValue left, EconomyValue right)
        {
            return new EconomyValue(left.RawValue - right.RawValue);
        }

        public static bool operator >(EconomyValue left, EconomyValue right)
        {
            return left.RawValue > right.RawValue;
        }

        public static bool operator <(EconomyValue left, EconomyValue right)
        {
            return left.RawValue < right.RawValue;
        }

        public static bool operator >=(EconomyValue left, EconomyValue right)
        {
            return left.RawValue >= right.RawValue;
        }

        public static bool operator <=(EconomyValue left, EconomyValue right)
        {
            return left.RawValue <= right.RawValue;
        }

        public static implicit operator EconomyValue(int value)
        {
            return new EconomyValue(value);
        }

        public static implicit operator EconomyValue(long value)
        {
            return new EconomyValue(value);
        }

        public static explicit operator long(EconomyValue value)
        {
            return value.RawValue;
        }
    }
}
