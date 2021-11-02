namespace Core.Framework.Nav.AStar
{
    public struct Point2D {
        public int X,
                   Y;
        public Point2D(int x, int y) {
            X = x;
            Y = y;
        }
    }
    public struct Point3D {
        public int X,
                   Y,
                   Z;
        public Point3D(int x, int y, int z) {
            X = x;
            Y = y;
            Z = z;
        }
    }
}
