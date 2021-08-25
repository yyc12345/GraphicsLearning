using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Radium.RayTracing {
    public class BoundingBox {

        public BoundingBox() {
            is_init = false;
            max = new Point3D();
            min = new Point3D();
        }

        public bool is_init;
        public Point3D max;
        public Point3D min;

        public void NewPoint(Point3D p) {
            if (!is_init) {
                is_init = true;
                max.x = min.x = p.x;
                max.y = min.y = p.y;
                max.z = min.z = p.z;
            } else {
                if (max.x < p.x) max.x = p.x;
                if (min.x > p.x) min.x = p.x;

                if (max.y < p.y) max.y = p.y;
                if (min.y > p.y) min.y = p.y;

                if (max.z < p.z) max.z = p.z;
                if (min.z > p.z) min.z = p.z;
            }
        }

        // 将光线原点作为起点x0, y0, z0，光线方向作为delta x, y, z
        // 可以有直线参数方程x = x0 + t * delta_x ...
        // 对于xyz每一对slab，分别令其对应坐标数值带入参数方程求得t，其他两组坐标由于是0不需要带入
        public bool IsIntersected(Beam ray) {
            if (!is_init) throw new Exception("Unfinished bounding box!");

            bool[] is_zero = new bool[3];
            is_zero[0] = ray.direction.x == 0;
            is_zero[1] = ray.direction.y == 0;
            is_zero[2] = ray.direction.z == 0;

            // 计算每组slab
            var t_max = new double[3];
            var t_min = new double[3];

            if (is_zero[0]) {
                // delta is zero
                t_max[0] = t_min[0] = 0;
            } else {
                t_max[0] = (max.x - ray.source.x) / ray.direction.x;
                t_min[0] = (min.x - ray.source.x) / ray.direction.x;
            }

            if (is_zero[1]) {
                // delta is zero
                t_max[1] = t_min[1] = 0;
            } else {
                t_max[1] = (max.y - ray.source.y) / ray.direction.y;
                t_min[1] = (min.y - ray.source.y) / ray.direction.y;
            }

            if (is_zero[2]) {
                // delta is zero
                t_max[2] = t_min[2] = 0;
            } else {
                t_max[2] = (max.z - ray.source.z) / ray.direction.z;
                t_min[2] = (min.z - ray.source.z) / ray.direction.z;
            }

            // 计算t max min，同时如果对应delta=0，即对应面平行无效，则不考虑
            // return max and min
            double tmax = 0, tmin = 0;
            bool has_picked = false;
            double cache;
            for (int i = 0; i < 3; i++) {
                if (is_zero[i]) continue;

                // first, try correcting t_max and t_min
                if (t_max[i] < t_min[i]) {
                    cache = t_max[i];
                    t_max[i] = t_min[i];
                    t_min[i] = cache;
                }

                // get max and min
                if (!has_picked) {
                    has_picked = true;
                    tmax = t_max[i];
                    tmin = t_min[i];
                } else {
                    if (tmax > t_max[i]) tmax = t_max[i];
                    if (tmin < t_min[i]) tmin = t_min[i];
                }
            }
            if (!has_picked) throw new Exception("Impossible boundingbox");

            // 保证tmin <= tmax，且都不是负数。负数表示反向相交，不保留；一正一负表示视点在物体内部，仍然保留
            // 等于是为了平面也可以渲染的要求
            // consideration
            if (tmin > tmax) return false;
            //if (tmin < 0 && tmax < 0) return false;
            return true;
        }

    }
}
