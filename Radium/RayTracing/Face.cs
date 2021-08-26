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
        // used in weight
        double weight_denominator;
        // used in intersection
        Vector3D e1, e2;
        // used in normalmap
        Vector3D TBN_T;

        public void FillData(MeshObject obj, Scene scene) {
            p1 = obj.vecList[v1];
            p2 = obj.vecList[v2];
            p3 = obj.vecList[v3];

            nml1 = obj.normalList[vn1];
            nml2 = obj.normalList[vn2];
            nml3 = obj.normalList[vn3];

            uv1 = obj.uvList[vt1];
            uv2 = obj.uvList[vt2];
            uv3 = obj.uvList[vt3];

            if (useMaterial) material = scene.materialList[matIndex];
            else material = UtilFunc.DEFAULT_MATERIAL;

            // construct some internal variables
            var mat = new Matrix3x3(new Vector3D(p1), new Vector3D(p2), new Vector3D(p3), true);
            this.weight_denominator = mat.Det();
            if (this.weight_denominator == 0) throw new Exception("GetWeight() zero denominator");

            this.e1 = p1 - p2;
            this.e2 = p1 - p3;

            var uv12 = uv2 - uv1;
            var uv13 = uv3 - uv1;
            var edge12 = p2 - p1;
            var edge13 = p3 - p1;
            var tbn_denominator = 1.0 / (uv12.x * uv13.y - uv13.x * uv12.y);
            TBN_T = new Vector3D(
                tbn_denominator * (uv13.y * edge12.x - uv12.y * edge13.x),
                tbn_denominator * (uv13.y * edge12.y - uv12.y * edge13.y),
                tbn_denominator * (uv13.y * edge12.z - uv12.y * edge13.z)
            );
            TBN_T.SetUnit();
        }

#if DEBUG
        public Vector3D GetInternalPointNormal(Point3D p, double w1, double w2, double w3, Radium.Utils.TracingDebug debug, bool need_draw) {
#else
        public Vector3D GetInternalPointNormal(Point3D p, double w1, double w2, double w3) {
#endif
            var intersected_point = nml1 * w1 + nml2 * w2 + nml3 * w3;
            intersected_point.SetUnit();

            // if mat don't use normalmap, return directly
            if (material.normalmap_texture == null) return intersected_point;

            // get normalmap and TBN matrix
            var intersected_uv = uv1 * w1 + uv2 * w2 + uv3 * w3;
            var normalmap = material.normalmap_texture.GetPixel(intersected_uv);

            // 判断三角形面序和实际法线方向是否相反，相反就利用纠正的法线(取反)作为TBN_N
            var tmp = e1 ^ e2;
            Vector3D TBN_N = intersected_point;
            if (tmp * intersected_point <= 0) TBN_N = -intersected_point;

            var TBN_B = TBN_T ^ TBN_N;
            TBN_B.SetUnit();
            var TBN = new Matrix3x3(TBN_T, TBN_B, TBN_N, true);

#if DEBUG
            if (need_draw) {
                debug.NewVector(p, TBN_T);
                debug.NewVector(p, TBN_B);
            }
#endif

            // normalmap 0~1 => -1~1
            normalmap = normalmap * 2.0 - 1.0;
            normalmap.SetUnit();
            // from local space to tangent space
            normalmap = TBN * normalmap;
            normalmap.SetUnit();
            return normalmap;
        }

        public Color GetDiffuse(Point3D p, double w1, double w2, double w3) {
            if (material.base_color_texture == null) return material.diffuse;

            // get intersect uv
            var intersected_uv = uv1 * w1 + uv2 * w2 + uv3 * w3;
            return new Color(material.base_color_texture.GetPixel(intersected_uv));
        }

        public void GetWeight(Point3D p, out double w1, out double w2, out double w3) {
            w1 = w2 = w3 = 0;

            // have calculated
            //var mat = new Matrix3x3(new Vector3D(p1), new Vector3D(p2), new Vector3D(p3), true);
            //var denominator = mat.Det();
            //if (denominator == 0) throw new Exception("GetWeight() zero denominator");

            var mat = new Matrix3x3(new Vector3D(p), new Vector3D(p2), new Vector3D(p3), true);
            w1 = mat.Det() / weight_denominator;
            mat = new Matrix3x3(new Vector3D(p1), new Vector3D(p), new Vector3D(p3), true);
            w2 = mat.Det() / weight_denominator;
            mat = new Matrix3x3(new Vector3D(p1), new Vector3D(p2), new Vector3D(p), true);
            w3 = mat.Det() / weight_denominator;

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

            // have calculated
            //var e1 = p1 - p2;
            //var e2 = p1 - p3;
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

            if (t <= UtilFunc.TOLERANCE) return false;
            if (beta < 0 || beta > 1 || gamma < 0 || gamma > 1 || gamma + beta > 1) return false;

            intersection = ray.source + (ray.direction * t);
            return true;
        }
    }
}
