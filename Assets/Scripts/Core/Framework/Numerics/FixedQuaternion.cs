
namespace Core.Framework.Numerics
{
    public struct FixedQuaternion
    {
        public static FixedQuaternion identity { get; } = UnityEngine.Quaternion.identity;

        public FixedNumber x;
        public FixedNumber y;
        public FixedNumber z;
        public FixedNumber w;

        public FixedVector3 eulerAngles
        {
            get
            {
                UnityEngine.Quaternion q = this;
                return q.eulerAngles;
            }
            set
            {
                var q = Euler(value);
                Set(q.x, q.y, q.z, q.w);
            }
        }

        public FixedQuaternion normalized => Normalize(this);

        public FixedQuaternion(FixedNumber x, FixedNumber y, FixedNumber z, FixedNumber w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }

        public static FixedNumber Angle(FixedQuaternion a, FixedQuaternion b) => UnityEngine.Quaternion.Angle(a, b);

        public static FixedQuaternion AngleAxis(FixedNumber angle, FixedVector3 axis) => UnityEngine.Quaternion.AngleAxis(angle, axis);

        public static FixedNumber Dot(FixedQuaternion a, FixedQuaternion b) => UnityEngine.Quaternion.Dot(a, b);

        public static FixedQuaternion Euler(FixedVector3 euler) => UnityEngine.Quaternion.Euler(euler);

        public static FixedQuaternion Euler(FixedNumber x, FixedNumber y, FixedNumber z) => UnityEngine.Quaternion.Euler(x, y, z);
        public static FixedQuaternion FromToRotation(FixedVector3 fromDirection, FixedVector3 toDirection)
            => UnityEngine.Quaternion.FromToRotation(fromDirection, toDirection);
        public static FixedQuaternion Inverse(FixedQuaternion rotation) => UnityEngine.Quaternion.Inverse(rotation);
        public static FixedQuaternion Lerp(FixedQuaternion a, FixedQuaternion b, FixedNumber t)
            => UnityEngine.Quaternion.Lerp(a, b, t);
        public static FixedQuaternion LerpUnclamped(FixedQuaternion a, FixedQuaternion b, FixedNumber t)
            => UnityEngine.Quaternion.LerpUnclamped(a, b, t);
        public static FixedQuaternion LookRotation(FixedVector3 forward) => UnityEngine.Quaternion.LookRotation(forward);
        public static FixedQuaternion LookRotation(FixedVector3 forward, FixedVector3 upwards)
            => UnityEngine.Quaternion.LookRotation(forward, upwards);
        public static FixedQuaternion Normalize(FixedQuaternion q) => UnityEngine.Quaternion.Normalize(q);
        public static FixedQuaternion RotateTowards(FixedQuaternion from, FixedQuaternion to, FixedNumber maxDegreesDelta)
            => UnityEngine.Quaternion.RotateTowards(from, to, maxDegreesDelta);
        public static FixedQuaternion Slerp(FixedQuaternion a, FixedQuaternion b, FixedNumber t)
            => UnityEngine.Quaternion.Slerp(a, b, t);
        public static FixedQuaternion SlerpUnclamped(FixedQuaternion a, FixedQuaternion b, FixedNumber t)
            => UnityEngine.Quaternion.SlerpUnclamped(a, b, t);

        public override bool Equals(object obj)
        {
            if(obj is FixedQuaternion)
            {
                var other = (FixedQuaternion) obj;
                return this == other;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        public void Normalize()
        {
            var q = Normalize(this);
            Set(q.x, q.y, q.z, q.w);
        }

        public void Set(FixedNumber newX, FixedNumber newY, FixedNumber newZ, FixedNumber newW)
        {
            x = newX;
            y = newY;
            z = newZ;
            w = newW;
        }

        public void SetFromToRotation(FixedVector3 fromDirection, FixedVector3 toDirection)
        {
            UnityEngine.Quaternion q = this;
            q.SetFromToRotation(fromDirection, toDirection);
            Set(q.x, q.y, q.z, q.w);
        }

        public void SetLookRotation(FixedVector3 view)
        {
            UnityEngine.Quaternion q = this;
            q.SetLookRotation(view);
            Set(q.x, q.y, q.z, q.w);
        }

        public void SetLookRotation(FixedVector3 view, FixedVector3 up)
        {
            UnityEngine.Quaternion q = this;
            q.SetLookRotation(view, up);
            Set(q.x, q.y, q.z, q.w);
        }

        public void ToAngleAxis(out FixedNumber angle, out FixedVector3 axis)
        {
            float tmpAngle;
            UnityEngine.Vector3 tmpAxis;
            UnityEngine.Quaternion q = this;
            q.ToAngleAxis(out tmpAngle, out tmpAxis);
            angle = tmpAngle;
            axis = tmpAxis;
        }

        public override string ToString()
        {
            return $"({x}, {y}, {z}, {w})";
        }

        public static FixedVector3 operator *(FixedQuaternion rotation, FixedVector3 point)
        {
            UnityEngine.Quaternion r = rotation;
            UnityEngine.Vector3 p = point;
            return r * p;
        }

        public static FixedQuaternion operator *(FixedQuaternion lhs, FixedQuaternion rhs)
        {
            UnityEngine.Quaternion l = lhs;
            UnityEngine.Quaternion r = rhs;
            return l * r;
        }

        public static bool operator ==(FixedQuaternion lhs, FixedQuaternion rhs)
            => lhs.x == rhs.x && lhs.y == rhs.y && lhs.z == rhs.z && lhs.w == rhs.w;
        public static bool operator !=(FixedQuaternion lhs, FixedQuaternion rhs)
            => lhs.x != rhs.x || lhs.y != rhs.y || lhs.z != rhs.z || lhs.w != rhs.w;


        public static implicit operator UnityEngine.Quaternion(FixedQuaternion d) => new UnityEngine.Quaternion(d.x, d.y, d.z, d.w);
        public static implicit operator FixedQuaternion(UnityEngine.Quaternion d) => new FixedQuaternion(d.x, d.y, d.z, d.w);
        public static implicit operator System.Numerics.Quaternion(FixedQuaternion d) => new System.Numerics.Quaternion(d.x, d.y, d.z, d.w);
        public static implicit operator FixedQuaternion(System.Numerics.Quaternion d) => new FixedQuaternion(d.X, d.Y, d.Z, d.W);
    }
}
