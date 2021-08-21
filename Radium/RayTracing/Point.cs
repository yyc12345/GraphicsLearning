using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Radium.RayTracing {
    public class Point3D {
        public double x, y, z;

        public Point3D(Point3D p) { x = p.x; y = p.y; z = p.z; }
        public Point3D() { x = y = z = 0; }
        public Point3D(double newx, double newy, double newz) { x = newx; y = newy; z = newz; }
        public Point3D(Vector3D v) { x = v.x; y = v.y; z = v.z; }

        //点加向量
        public static Point3D operator +(Point3D u, Vector3D v) { return new Point3D(u.x + v.x, u.y + v.y, u.z + v.z); }
        //点减向量
        public static Point3D operator -(Point3D u, Vector3D v) { return new Point3D(u.x - v.x, u.y - v.y, u.z - v.z); }
        //两点相减
        public static Vector3D operator -(Point3D u, Point3D v) { return new Vector3D(u.x - v.x, u.y - v.y, u.z - v.z); }
        //单目减
        public static Point3D operator -(Point3D u) { return new Point3D(-u.x, -u.y, -u.z); }

        //判等(不等)
        public static bool operator ==(Point3D u, Point3D v) {
            return u.x == v.x && u.y == v.y && u.z == v.z;
        }
        public static bool operator !=(Point3D u, Point3D v) {
            return !(u == v);
        }
    }

    public class Point2D {
        public double x, y;

        public Point2D(Point2D p) { x = p.x; y = p.y; }
        public Point2D() { x = y = 0; }
        public Point2D(double newx, double newy) { x = newx; y = newy; }
        public Point2D(Vector2D v) { x = v.x; y = v.y; }

        //点加向量
        public static Point2D operator +(Point2D u, Vector3D v) { return new Point2D(u.x + v.x, u.y + v.y); }
        //点减向量
        public static Point2D operator -(Point2D u, Vector3D v) { return new Point2D(u.x - v.x, u.y - v.y); }
        //两点相减
        public static Vector2D operator -(Point2D u, Point2D v) { return new Vector2D(v.x - u.x, v.y - u.y); }
        //单目减
        public static Point2D operator -(Point2D u) { return new Point2D(-u.x, -u.y); }

        //判等(不等)
        public static bool operator ==(Point2D u, Point2D v) {
            return u.x == v.x && u.y == v.y;
        }
        public static bool operator !=(Point2D u, Point2D v) {
            return !(u == v);
        }
    }

}
