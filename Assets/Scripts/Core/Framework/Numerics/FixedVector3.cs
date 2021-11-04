namespace Core.Framework.Numerics
{
    public struct FixedVector3
    {
        public static FixedVector3 right { get; } = new FixedVector3(1, 0, 0);
        public static FixedVector3 left { get; } = new FixedVector3(-1, 0, 0);
        public static FixedVector3 up { get; } = new FixedVector3(0, 1, 0);
        public static FixedVector3 back { get; } = new FixedVector3(0, 0, -1);
        public static FixedVector3 forward { get; } = new FixedVector3(0, 0, 1);
        public static FixedVector3 one { get; } = new FixedVector3(1, 1, 1);
        public static FixedVector3 zero { get; } = new FixedVector3(0, 0, 0);
        public static FixedVector3 negativeInfinity { get; } = new FixedVector3(int.MinValue, int.MinValue, int.MinValue);
        public static FixedVector3 positiveInfinity { get; } = new FixedVector3(int.MaxValue, int.MaxValue, int.MaxValue);
        public static FixedVector3 down { get; } = new FixedVector3(0, -1, 0);

        public FixedNumber x;
        public FixedNumber y;
        public FixedNumber z;

        public FixedVector3 normalized => UnityEngine.Vector3.Normalize(this);
        public FixedNumber magnitude => UnityEngine.Vector3.Magnitude(this);
        public FixedNumber sqrMagnitude => UnityEngine.Vector3.SqrMagnitude(this);

        public FixedVector3(FixedNumber x, FixedNumber y, FixedNumber z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public static FixedNumber Angle(FixedVector3 from, FixedVector3 to) => UnityEngine.Vector3.Angle(from, to);
        public static FixedVector3 ClampMagnitude(FixedVector3 vector, FixedNumber maxLength)
            => UnityEngine.Vector3.ClampMagnitude(vector, maxLength);
        public static FixedVector3 Cross(FixedVector3 lhs, FixedVector3 rhs) => UnityEngine.Vector3.Cross(lhs, rhs);
        public static FixedNumber Distance(FixedVector3 a, FixedVector3 b) => UnityEngine.Vector3.Distance(a, b);
        public static FixedNumber Dot(FixedVector3 lhs, FixedVector3 rhs) => UnityEngine.Vector3.Dot(lhs, rhs);
        public static FixedVector3 Lerp(FixedVector3 a, FixedVector3 b, FixedNumber t) => UnityEngine.Vector3.Lerp(a, b, t);
        public static FixedVector3 LerpUnclamped(FixedVector3 a, FixedVector3 b, FixedNumber t) => UnityEngine.Vector3.LerpUnclamped(a, b, t);
        public static FixedNumber Magnitude(FixedVector3 vector) => UnityEngine.Vector3.Magnitude(vector);
        public static FixedVector3 Max(FixedVector3 lhs, FixedVector3 rhs) => UnityEngine.Vector3.Max(lhs, rhs);
        public static FixedVector3 Min(FixedVector3 lhs, FixedVector3 rhs) => UnityEngine.Vector3.Min(lhs, rhs);
        public static FixedVector3 MoveTowards(FixedVector3 current, FixedVector3 target, FixedNumber maxDistanceDelta)
            => UnityEngine.Vector3.MoveTowards(current, target, maxDistanceDelta);
        public static FixedVector3 Normalize(FixedVector3 value) => UnityEngine.Vector3.Normalize(value);
        public static void OrthoNormalize(ref FixedVector3 normal, ref FixedVector3 tangent, ref FixedVector3 binormal)
        {
            UnityEngine.Vector3 tmpNormal = normal;
            UnityEngine.Vector3 tmpTangent = tangent;
            UnityEngine.Vector3 tmpBinormal = binormal;
            UnityEngine.Vector3.OrthoNormalize(ref tmpNormal, ref tmpTangent, ref tmpBinormal);
            normal = tmpNormal;
            tangent = tmpTangent;
            binormal = tmpBinormal;
        }
        public static void OrthoNormalize(ref FixedVector3 normal, ref FixedVector3 tangent)
        {
            UnityEngine.Vector3 tmpNormal = normal;
            UnityEngine.Vector3 tmpTangent = tangent;
            UnityEngine.Vector3.OrthoNormalize(ref tmpNormal, ref tmpTangent);
            normal = tmpNormal;
            tangent = tmpTangent;
        }
        public static FixedVector3 Project(FixedVector3 vector, FixedVector3 onNormal)
            => UnityEngine.Vector3.Project(vector, onNormal);
        public static FixedVector3 ProjectOnPlane(FixedVector3 vector, FixedVector3 planeNormal)
            => UnityEngine.Vector3.ProjectOnPlane(vector, planeNormal);
        public static FixedVector3 Reflect(FixedVector3 inDirection, FixedVector3 inNormal)
            => UnityEngine.Vector3.Reflect(inDirection, inNormal);
        public static FixedVector3 RotateTowards(FixedVector3 current, FixedVector3 target, FixedNumber maxRadiansDelta, FixedNumber maxMagnitudeDelta)
            => UnityEngine.Vector3.RotateTowards(current, target, maxRadiansDelta, maxMagnitudeDelta);
        public static FixedVector3 Scale(FixedVector3 a, FixedVector3 b) => UnityEngine.Vector3.Scale(a, b);
        public static FixedNumber SignedAngle(FixedVector3 from, FixedVector3 to, FixedVector3 axis)
            => UnityEngine.Vector3.SignedAngle(from, to, axis);
        public static FixedVector3 Slerp(FixedVector3 a, FixedVector3 b, FixedNumber t) => UnityEngine.Vector3.Slerp(a, b, t);
        public static FixedVector3 SlerpUnclamped(FixedVector3 a, FixedVector3 b, FixedNumber t) => UnityEngine.Vector3.SlerpUnclamped(a, b, t);
        public static FixedVector3 SmoothDamp(FixedVector3 current, FixedVector3 target, ref FixedVector3 currentVelocity, FixedNumber smoothTime)
        {
            UnityEngine.Vector3 tmpCurrentVelocity = currentVelocity;
            var ret = UnityEngine.Vector3.SmoothDamp(current, target, ref tmpCurrentVelocity, smoothTime);
            currentVelocity = tmpCurrentVelocity;
            return ret;
        }
        public static FixedVector3 SmoothDamp(FixedVector3 current, FixedVector3 target, ref FixedVector3 currentVelocity,
            FixedNumber smoothTime, FixedNumber maxSpeed)
        {
            UnityEngine.Vector3 tmpCurrentVelocity = currentVelocity;
            var ret = UnityEngine.Vector3.SmoothDamp(current, target, ref tmpCurrentVelocity, smoothTime, maxSpeed);
            currentVelocity = tmpCurrentVelocity;
            return ret;
        }
        public static FixedVector3 SmoothDamp(FixedVector3 current, FixedVector3 target, ref FixedVector3 currentVelocity,
            FixedNumber smoothTime, FixedNumber maxSpeed, FixedNumber deltaTime)
        {
            UnityEngine.Vector3 tmpCurrentVelocity = currentVelocity;
            var ret = UnityEngine.Vector3.SmoothDamp(current, target, ref tmpCurrentVelocity, smoothTime, maxSpeed, deltaTime);
            currentVelocity = tmpCurrentVelocity;
            return ret;
        }
        public static FixedNumber SqrMagnitude(FixedVector3 vector) => UnityEngine.Vector3.SqrMagnitude(vector);

        public static FixedVector3 operator +(FixedVector3 a) => a;
        public static FixedVector3 operator -(FixedVector3 a) => new FixedVector3(-a.x, -a.y, -a.z);
        public static FixedVector3 operator +(FixedVector3 a, FixedVector3 b) => new FixedVector3(a.x + b.x, a.y + b.y, a.z + b.z);
        public static FixedVector3 operator -(FixedVector3 a, FixedVector3 b) => new FixedVector3(a.x - b.x, a.y - b.y, a.z - b.z);
        public static FixedVector3 operator *(FixedNumber d, FixedVector3 a) => new FixedVector3(a.x * d, a.y * d, a.z * d);
        public static FixedVector3 operator *(FixedVector3 a, FixedNumber d) => new FixedVector3(a.x * d, a.y * d, a.z * d);
        public static FixedVector3 operator /(FixedVector3 a, FixedNumber d)
        {
            if (d == 0)
            {
                throw new System.DivideByZeroException();
            }
            return new FixedVector3(a.x / d, a.y / d, a.z / d);
        }

        public static implicit operator UnityEngine.Vector3(FixedVector3 d) => new UnityEngine.Vector3(d.x, d.y, d.z);
        public static implicit operator FixedVector3(UnityEngine.Vector3 d) => new FixedVector3(d.x, d.y, d.z);
        public static implicit operator System.Numerics.Vector3(FixedVector3 d) => new System.Numerics.Vector3(d.x, d.y, d.z);
        public static implicit operator FixedVector3(System.Numerics.Vector3 d) => new FixedVector3(d.X, d.Y, d.Z);
        public override string ToString()
        {
            return $"({x}, {y}, {z})";
        }
    }
}
