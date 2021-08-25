using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Radium.RayTracing {
    public class Face {
        public Face(UInt32 v1, UInt32 vn1, UInt32 vt1, UInt32 v2, UInt32 vn2, UInt32 vt2, UInt32 v3, UInt32 vn3, UInt32 vt3, bool useMat, UInt32 matIndex) {
            this.v1 = v1;
            this.v2 = v2;
            this.v3 = v3;            
            this.vn1 = vn1;
            this.vn2 = vn2;
            this.vn3 = vn3;            
            this.vt1 = vt1;
            this.vt2 = vt2;
            this.vt3 = vt3;

            this.useMaterial = useMat;
            this.matIndex = matIndex;
        }

        public UInt32 v1, v2, v3, vn1, vn2, vn3, vt1, vt2, vt3, matIndex;
        public bool useMaterial;
        public Point3D p1, p2, p3;
        public Vector3D nml1, nml2, nml3;
        public Point2D uv1, uv2, uv3;
        public Material material;

        // assitant variable for intersection calc
        // 两个基础平面向量
        Vector3D v12, v23;
        // 平面法向量和隐式方程参数D
        Vector3D faceN;
        double faceD;
        // 三角形面片面积
        double faceSquare;

        public void FillData(Point3D[] vecList, Vector3D[] nmlList, Point2D[] uvList, Material[] matList) {
            p1 = vecList[v1];
            p2 = vecList[v2];
            p3 = vecList[v3];

            nml1 = nmlList[vn1];
            nml2 = nmlList[vn2];
            nml3 = nmlList[vn3];

            uv1 = uvList[vt1];
            uv2 = uvList[vt2];
            uv3 = uvList[vt3];

            if (useMaterial) material = matList[matIndex];
            else material = UtilFunc.DEFAULT_MATERIAL;

            // construct some internal variables
            v12 = p2 - p1;
            v23 = p3 - p2;

            // 使用blender的右手坐标系，则使用右手定则，逆时针绕三角形面片为正向
            faceN = v12 ^ v23;
            faceN.SetUnit();
            faceD = -(faceN * new Vector3D(p1));

            var cache = (-v12) ^ v23;
            faceSquare = cache.GetLen() / 2;
        }

        public Vector3D GetInternalPointNormal(Point3D p) {
            GetWeight(p, out double w1, out double w2, out double w3);
            var intersected_point = nml1 * w1 + nml2 * w2 + nml3 * w3;
            intersected_point.SetUnit();
            return intersected_point;
        }

#if DEBUG
        public Color GetLocalColor(Beam ray, Point3D p, Light[] lightList, Radium.Utils.TracingDebug debug, bool need_draw) {
#else
        public Color GetLocalColor(Beam ray, Point3D p, Light[] lightList) {
#endif
            // ambient
            var result = material.ambient * UtilFunc.DEFAULT_AMBIENT;

            // calc normal
            var normal = GetInternalPointNormal(p);
#if DEBUG
            //if (need_draw)
            //    debug.NewVector(p, normal);
#endif

            // for each light, calc diffuse and specular
            foreach (var light in lightList) {
                var L = light.GetDirectionFromPointToSource(p);
#if DEBUG
                if (need_draw && light is SunLight)
                    debug.NewVector(p, L);
#endif
                var V =-ray.direction;
                var LN = L * normal;
                if (LN < 0) continue;
                result = result + (light.GetColor(p) * material.diffuse * LN);

                V.SetUnit();
                var H = L + V;
                H.SetUnit();

                var HN = H * normal;
                if (HN < 0) continue;
                result = result + (light.GetColor(p) * material.specular *
                    Math.Pow(HN, material.specularN));
            }

            return result;
        }

        public void GetWeight(Point3D p, out double w1, out double w2, out double w3) {
            w1 = w2 = w3 = 0;

            var mat = new Matrix3x3(new Vector3D(p1), new Vector3D(p2), new Vector3D(p3), true);
            var denominator = mat.Det();
            if (denominator == 0) throw new Exception("GetWeight() zero denominator");

            mat = new Matrix3x3(new Vector3D(p), new Vector3D(p2), new Vector3D(p3), true);
            w1 = mat.Det() / denominator;
            mat = new Matrix3x3(new Vector3D(p1), new Vector3D(p), new Vector3D(p3), true);
            w2 = mat.Det() / denominator;
            mat = new Matrix3x3(new Vector3D(p1), new Vector3D(p2), new Vector3D(p), true);
            w3 = mat.Det() / denominator;

            if (w1 < 0 || w1 > 1 || w2 < 0 || w2 > 1 || w3 < 0 || w3 > 1) {
                var cache = new Vector3D(w1, w2, w3);
                cache.SetUnit();
                w1 = cache.x;
                w2 = cache.y;
                w3 = cache.z;
            }
            //if (!UtilFunc.CloseBy(w1 + w2 + w3, 1)) throw new Exception("GetWeight() irrlegal w1 w2 w3");
        }

        public bool GetIntersection(Beam ray, out double t, out Point3D intersection) {
            /*
            // t = -( D + n * R0) / (n * Rd)
            var cache = faceN * ray.direction;
            if (cache < UtilFunc.TOLERANCE) {
                // 平行，没有交点
                t = 0;
                intersection = null;
                return false;
            }
            t = -(faceD + faceN * new Vector3D(ray.source)) / cache;
            if (t <= 0) {
                intersection = null;
                return false;
            }

            intersection = ray.source + ray.direction * t;

            // we get the 

            return true;
            */

            t = 0;
            intersection = null;

            var e1 = p1 - p2;
            var e2 = p1 - p3;
            var s = p1 - ray.source;

            var mat = new Matrix3x3(ray.direction, e1, e2, true);
            var denominator = mat.Det();
            if (denominator == 0) return false;

            mat = new Matrix3x3(s, e1, e2, true);
            t = mat.Det() / denominator;
            mat = new Matrix3x3(ray.direction, s, e2, true);
            var beta = mat.Det() / denominator;
            mat = new Matrix3x3(ray.direction, e1, s, true);
            var gamma = mat.Det() / denominator;

            if (t <= 0) return false;
            if (beta < 0 || beta > 1 || gamma < 0 || gamma > 1 || gamma + beta > 1) return false;

            intersection = ray.source + (ray.direction * t);
            return true;
        }
    }
}
