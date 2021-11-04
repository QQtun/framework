
namespace Core.Framework.Numerics
{
    public struct FixedVector2
    {
        public static FixedVector2 right { get; } = new FixedVector2(1, 0);
        public static FixedVector2 left { get; } = new FixedVector2(-1, 0);
        public static FixedVector2 up { get; } = new FixedVector2(0, 1);
        public static FixedVector2 one { get; } = new FixedVector2(1, 1);
        public static FixedVector2 zero { get; } = new FixedVector2(0, 0);
        public static FixedVector2 negativeInfinity { get; } = new FixedVector2(int.MinValue, int.MinValue);
        public static FixedVector2 positiveInfinity { get; } = new FixedVector2(int.MaxValue, int.MaxValue);
        public static FixedVector2 down { get; } = new FixedVector2(0, -1);

        public FixedNumber x;
        public FixedNumber y;

        public FixedVector2 normalized => ((UnityEngine.Vector2)this).normalized;
        public FixedNumber magnitude => ((UnityEngine.Vector2)this).magnitude;
        public FixedNumber sqrMagnitude => UnityEngine.Vector2.SqrMagnitude(this);

        public FixedVector2(FixedNumber x, FixedNumber y)
        {
            this.x = x;
            this.y = y;
        }

        public static FixedNumber Angle(FixedVector2 from, FixedVector2 to) => UnityEngine.Vector2.Angle(from, to);
        public static FixedVector2 ClampMagnitude(FixedVector2 vector, FixedNumber maxLength)
            => UnityEngine.Vector2.ClampMagnitude(vector, maxLength);
        public static FixedNumber Distance(FixedVector2 a, FixedVector2 b) => UnityEngine.Vector2.Distance(a, b);
        public static FixedNumber Dot(FixedVector2 lhs, FixedVector2 rhs) => UnityEngine.Vector2.Dot(lhs, rhs);
        public static FixedVector2 Lerp(FixedVector2 a, FixedVector2 b, FixedNumber t) => UnityEngine.Vector2.Lerp(a, b, t);
        public static FixedVector2 LerpUnclamped(FixedVector2 a, FixedVector2 b, FixedNumber t) => UnityEngine.Vector2.LerpUnclamped(a, b, t);
        public static FixedNumber Magnitude(FixedVector2 vector) => ((UnityEngine.Vector2)vector).magnitude;
        public static FixedVector2 Max(FixedVector2 lhs, FixedVector2 rhs) => UnityEngine.Vector2.Max(lhs, rhs);
        public static FixedVector2 Min(FixedVector2 lhs, FixedVector2 rhs) => UnityEngine.Vector2.Min(lhs, rhs);
        public static FixedVector2 MoveTowards(FixedVector2 current, FixedVector2 target, FixedNumber maxDistanceDelta)
            => UnityEngine.Vector2.MoveTowards(current, target, maxDistanceDelta);
        public static FixedVector2 Perpendicular(FixedVector2 inDirection) => UnityEngine.Vector2.Perpendicular(inDirection);
        public static FixedVector2 Normalize(FixedVector2 value) => ((UnityEngine.Vector2)value).normalized;
        public static FixedVector2 Reflect(FixedVector2 inDirection, FixedVector2 inNormal)
            => UnityEngine.Vector2.Reflect(inDirection, inNormal);
        public static FixedVector2 Scale(FixedVector2 a, FixedVector2 b) => UnityEngine.Vector2.Scale(a, b);
        public static FixedVector2 SmoothDamp(FixedVector2 current, FixedVector2 target, ref FixedVector2 currentVelocity, FixedNumber smoothTime)
        {
            UnityEngine.Vector2 tmpCurrentVelocity = currentVelocity;
            var ret = UnityEngine.Vector2.SmoothDamp(current, target, ref tmpCurrentVelocity, smoothTime);
            currentVelocity = tmpCurrentVelocity;
            return ret;
        }
        public static FixedVector2 SmoothDamp(FixedVector2 current, FixedVector2 target, ref FixedVector2 currentVelocity,
            FixedNumber smoothTime, FixedNumber maxSpeed)
        {
            UnityEngine.Vector2 tmpCurrentVelocity = currentVelocity;
            var ret = UnityEngine.Vector2.SmoothDamp(current, target, ref tmpCurrentVelocity, smoothTime, maxSpeed);
            currentVelocity = tmpCurrentVelocity;
            return ret;
        }
        public static FixedVector2 SmoothDamp(FixedVector2 current, FixedVector2 target, ref FixedVector2 currentVelocity,
            FixedNumber smoothTime, FixedNumber maxSpeed, FixedNumber deltaTime)
        {
            UnityEngine.Vector2 tmpCurrentVelocity = currentVelocity;
            var ret = UnityEngine.Vector2.SmoothDamp(current, target, ref tmpCurrentVelocity, smoothTime, maxSpeed, deltaTime);
            currentVelocity = tmpCurrentVelocity;
            return ret;
        }
        public static FixedNumber SqrMagnitude(FixedVector2 vector) => UnityEngine.Vector2.SqrMagnitude(vector);

        public static FixedVector2 operator +(FixedVector2 a) => a;
        public static FixedVector2 operator -(FixedVector2 a) => new FixedVector2(-a.x, -a.y);
        public static FixedVector2 operator +(FixedVector2 a, FixedVector2 b) => new FixedVector2(a.x + b.x, a.y + b.y);
        public static FixedVector2 operator -(FixedVector2 a, FixedVector2 b) => new FixedVector2(a.x - b.x, a.y - b.y);
        public static FixedVector2 operator *(FixedNumber d, FixedVector2 a) => new FixedVector2(a.x * d, a.y * d);
        public static FixedVector2 operator *(FixedVector2 a, FixedNumber d) => new FixedVector2(a.x * d, a.y * d);
        public static FixedVector2 operator /(FixedVector2 a, FixedNumber d)
        {
            if (d == 0)
            {
                throw new System.DivideByZeroException();
            }
            return new FixedVector2(a.x / d, a.y / d);
        }

        public static implicit operator UnityEngine.Vector2(FixedVector2 d) => new UnityEngine.Vector2(d.x, d.y);
        public static implicit operator FixedVector2(UnityEngine.Vector2 d) => new FixedVector2(d.x, d.y);
        public static implicit operator System.Numerics.Vector2(FixedVector2 d) => new System.Numerics.Vector2(d.x, d.y);
        public static implicit operator FixedVector2(System.Numerics.Vector2 d) => new FixedVector2(d.X, d.Y);
        public override string ToString()
        {
            return $"({x}, {y})";
        }
    }
}
