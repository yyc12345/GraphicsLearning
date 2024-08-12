using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Radium.RayTracing {
    public class Matrix3x3 {
        public Matrix3x3(Vector3D v1, Vector3D v2, Vector3D v3, bool isColumnVector) {
            if (isColumnVector) {
                e11 = v1.x;
                e21 = v1.y;
                e31 = v1.z;

                e12 = v2.x;
                e22 = v2.y;
                e32 = v2.z;

                e13 = v3.x;
                e23 = v3.y;
                e33 = v3.z;
            } else {
                e11 = v1.x;
                e12 = v1.y;
                e13 = v1.z;

                e21 = v2.x;
                e22 = v2.y;
                e23 = v2.z;

                e31 = v3.x;
                e32 = v3.y;
                e33 = v3.z;
            }
        }

        public double e11, e12, e13;
        public double e21, e22, e23;
        public double e31, e32, e33;

        public double Det() {
            return e11 * e22 * e33 +
                e12 * e23 * e31 +
                e13 * e21 * e32 -
                e11 * e23 * e32 -
                e12 * e21 * e33 -
                e13 * e22 * e31;
        }

        //矩阵变换
        public static Vector3D operator *(Matrix3x3 m, Vector3D v) {
            return new Vector3D(
                m.e11 * v.x + m.e12 * v.y + m.e13 * v.z,
                m.e21 * v.x + m.e22 * v.y + m.e23 * v.z,
                m.e31 * v.x + m.e32 * v.y + m.e33 * v.z
            );
        }

    }
}
