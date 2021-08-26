using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Radium.RayTracing {
    public class Color {
        public double r, g, b;


        public Color(Vector3D p) { r = p.x; g = p.y; b = p.z; }
        public Color(Color p) { r = p.r; g = p.g; b = p.b; }
        public Color() { r = g = b = 0; }
        public Color(double newr, double newg, double newb) { r = newr; g = newg; b = newb; }

        //点加向量
        public static Color operator +(Color u, Color v) { return new Color(u.r + v.r, u.g + v.g, u.b + v.b); }
        //两点相减
        public static Color operator -(Color u, Color v) { return new Color(u.r - v.r, u.g - v.g, u.b - v.b); }
        public static Color operator *(Color u, double num) {
            return new Color(u.r * num, u.g * num, u.b * num);
        }
        public static Color operator *(Color u, Color v) {
            return new Color(u.r * v.r, u.g * v.g, u.b * v.b);
        }
        public static Color operator /(Color u, double num) {
            return new Color(u.r / num, u.g / num, u.b / num);
        }

        //判等(不等)
        public static bool operator ==(Color u, Color v) {
            return u.r == v.r && u.g == v.g && u.b == v.b;
        }
        public static bool operator !=(Color u, Color v) {
            return !(u == v);
        }
    }
}
