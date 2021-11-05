namespace Core.Framework.Numerics
{
    public struct FixedNumber
    {
        public const int TenKilo = 10000;

        private long _value;

        public FixedNumber(int value)
        {
            _value = value * TenKilo;
        }
        public FixedNumber(float value)
        {
            _value = (long)(value * TenKilo);
        }
        public FixedNumber(double value)
        {
            _value = (long)(value * TenKilo);
        }
        public override string ToString()
        {
            return _value.ToString();
        }

        public override bool Equals(object obj)
        {
            return _value.Equals(obj);
        }

        public override int GetHashCode()
        {
            return _value.GetHashCode();
        }

        public static FixedNumber operator +(FixedNumber a) => a;
        public static FixedNumber operator -(FixedNumber a)
        {
            var ret = new FixedNumber();
            ret._value = -a._value;
            return ret;
        }
        public static FixedNumber operator +(FixedNumber a, FixedNumber b)
        {
            var ret = new FixedNumber();
            ret._value = a._value + b._value;
            return ret;
        }
        public static FixedNumber operator -(FixedNumber a, FixedNumber b)
        {
            var ret = new FixedNumber();
            ret._value = a._value - b._value;
            return ret;
        }
        public static FixedNumber operator *(FixedNumber a, FixedNumber b)
        {
            var ret = new FixedNumber();
            ret._value = a._value * b._value;
            return ret;
        }
        public static FixedNumber operator /(FixedNumber a, FixedNumber b)
        {
            if (b._value == 0)
            {
                throw new System.DivideByZeroException();
            }
            var ret = new FixedNumber();
            ret._value = a._value / b._value;
            return new FixedNumber(ret);
        }

        public static implicit operator int(FixedNumber d) => (int)(d._value / 10000);
        public static implicit operator FixedNumber(int d) => new FixedNumber(d);
        public static implicit operator float(FixedNumber d) => d._value / 10000f;
        public static implicit operator FixedNumber(float d) => new FixedNumber(d);
        public static implicit operator double(FixedNumber d) => d._value / 10000d;
        public static implicit operator FixedNumber(double d) => new FixedNumber(d);

        public static bool operator ==(FixedNumber lhs, FixedNumber rhs) => lhs._value == rhs._value;
        public static bool operator !=(FixedNumber lhs, FixedNumber rhs) => lhs._value != rhs._value;
        public static bool operator ==(float lhs, FixedNumber rhs) => new FixedNumber(lhs)._value == rhs._value;
        public static bool operator !=(float lhs, FixedNumber rhs) => new FixedNumber(lhs)._value != rhs._value;
        public static bool operator ==(FixedNumber lhs, float rhs) => lhs._value == new FixedNumber(rhs)._value;
        public static bool operator !=(FixedNumber lhs, float rhs) => lhs._value != new FixedNumber(rhs)._value;
        public static bool operator ==(double lhs, FixedNumber rhs) => new FixedNumber(lhs)._value == rhs._value;
        public static bool operator !=(double lhs, FixedNumber rhs) => new FixedNumber(lhs)._value != rhs._value;
        public static bool operator ==(FixedNumber lhs, double rhs) => lhs._value == new FixedNumber(rhs)._value;
        public static bool operator !=(FixedNumber lhs, double rhs) => lhs._value != new FixedNumber(rhs)._value;
        public static bool operator ==(int lhs, FixedNumber rhs) => new FixedNumber(lhs)._value == rhs._value;
        public static bool operator !=(int lhs, FixedNumber rhs) => new FixedNumber(lhs)._value != rhs._value;
        public static bool operator ==(FixedNumber lhs, int rhs) => lhs._value == new FixedNumber(rhs)._value;
        public static bool operator !=(FixedNumber lhs, int rhs) => lhs._value != new FixedNumber(rhs)._value;
    }
}
