using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Radium.RayTracing {

    public class Vector3D {
        public double x, y, z;

        public Vector3D(Point3D p) { x = p.x; y = p.y; z = p.z; }
        public Vector3D(Vector3D p) { x = p.x; y = p.y; z = p.z; }
        public Vector3D() { x = y = z = 0; }
        public Vector3D(double newx, double newy, double newz) { x = newx; y = newy; z = newz; }

        //矢量加
        public static Vector3D operator +(Vector3D u, Vector3D v) { return new Vector3D(u.x + v.x, u.y + v.y, u.z + v.z); }
        //矢量减
        public static Vector3D operator -(Vector3D u, Vector3D v) { return new Vector3D(u.x - v.x, u.y - v.y, u.z - v.z); }
        //矢量乘(点积)
        public static double operator *(Vector3D u, Vector3D v) { return u.x * v.x + u.y * v.y + u.z * v.z; }
        //矢量乘(叉积)
        public static Vector3D operator ^(Vector3D u, Vector3D v) {
            return new Vector3D(u.y * v.z - u.z * v.y,
                            -u.x * v.z + u.z * v.x,
                              u.x * v.y - u.y * v.x);
        }
        //数乘
        public static Vector3D operator *(Vector3D u, double num) { return new Vector3D(u.x * num, u.y * num, u.z * num); }
        //数除
        public static Vector3D operator /(Vector3D u, double num) { return new Vector3D(u.x / num, u.y / num, u.z / num); }
        //单目减
        public static Vector3D operator -(Vector3D u) { return new Vector3D(-u.x, -u.y, -u.z); }

        //判等(不等)
        public static bool operator ==(Vector3D u, Vector3D v) {
            return u.x == v.x && u.y == v.y && u.z == v.z;
        }
        public static bool operator !=(Vector3D u, Vector3D v) {
            return !(u == v);
        }

        //设为单位矢量
        public double GetLen() {
            return (double)Math.Sqrt(x * x + y * y + z * z);
        }
        public void SetLen(double d) {
            SetUnit();
            x *= d; y *= d; z *= d;
        }
        public void SetUnit() {
            double len = GetLen();
            x /= len;
            y /= len;
            z /= len;
        }

    }

    public class Vector2D {
        public double x, y;

        public Vector2D(Vector2D p) { x = p.x; y = p.y; }
        public Vector2D() { x = y = 0; }
        public Vector2D(double newx, double newy) { x = newx; y = newy; }

        //矢量加
        public static Vector2D operator +(Vector2D u, Vector2D v) { return new Vector2D(u.x + v.x, u.y + v.y); }
        //矢量减
        public static Vector2D operator -(Vector2D u, Vector2D v) { return new Vector2D(u.x - v.x, u.y - v.y); }
        //矢量乘(点积)
        public static double operator *(Vector2D u, Vector2D v) { return u.x * v.x + u.y * v.y; }
        //数乘
        public static Vector2D operator *(Vector2D u, double num) { return new Vector2D(u.x * num, u.y * num); }
        //数除
        public static Vector2D operator /(Vector2D u, double num) { return new Vector2D(u.x / num, u.y / num); }
        //单目减
        public static Vector2D operator -(Vector2D u) { return new Vector2D(-u.x, -u.y); }

        //判等(不等)
        public static bool operator ==(Vector2D u, Vector2D v) {
            return u.x == v.x && u.y == v.y;
        }
        public static bool operator !=(Vector2D u, Vector2D v) {
            return !(u == v);
        }

        //设为单位矢量
        public double GetLen() {
            return Math.Sqrt(x * x + y * y);
        }
        public void SetLen(double d) {
            SetUnit();
            x *= d; y *= d;
        }
        public void SetUnit() {
            double len = GetLen();
            x /= len;
            y /= len;
        }

    }



}
