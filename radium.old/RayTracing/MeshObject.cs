using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Radium.RayTracing {
    public class MeshObject {
        public MeshObject(BinaryReader br) {
            boundingbox = new BoundingBox();

            var vecCount = br.ReadUInt32();
            vecList = new Point3D[vecCount];
            for(UInt32 i = 0; i < vecCount; i++) {
                var cache = new Point3D(br.ReadDouble(), br.ReadDouble(), br.ReadDouble());
                boundingbox.NewPoint(cache);
                vecList[i] = cache;
            }

            var uvCount = br.ReadUInt32();
            uvList = new Point2D[uvCount];
            for (UInt32 i = 0; i < uvCount; i++) {
                uvList[i] = new Point2D(br.ReadDouble(), br.ReadDouble());
            }

            var nmlCount = br.ReadUInt32();
            normalList = new Vector3D[nmlCount];
            for (UInt32 i = 0; i < nmlCount; i++) {
                normalList[i] = new Vector3D(br.ReadDouble(), br.ReadDouble(), br.ReadDouble());
            }

            var faceCount = br.ReadUInt32();
            faceList = new Face[faceCount];
            for (UInt32 i = 0; i < faceCount; i++) {
                faceList[i] = new Face(br.ReadUInt32(), br.ReadUInt32(), br.ReadUInt32(),
                    br.ReadUInt32(), br.ReadUInt32(), br.ReadUInt32(),
                    br.ReadUInt32(), br.ReadUInt32(), br.ReadUInt32(),
                    br.ReadByte().ToBoolean(), br.ReadUInt32());
            }
        }

        public Point3D[] vecList;
        public Vector3D[] normalList;
        public Point2D[] uvList;
        public Face[] faceList;
        public BoundingBox boundingbox;

        public void FillData(Scene scene) {
            foreach(var face in faceList) {
                face.FillData(this, scene);
            }
        }

        // 用于阴影检测，只要有焦点，就返回
        public bool HaveIntersection(Beam ray, double stop_value) {
            if (!boundingbox.IsIntersected(ray)) return false;

            Point3D vec;
            double t;
            foreach(var face in faceList) {
                if (face.GetIntersection(ray, out t, out vec)) {
                    if (t > stop_value) continue;   // if the face far than light, also continue, because ray.direction is normalized vector, so compare t and stop_value directly
                    else return true;
                }
            }
            return false;
        }

        // 用于求交，返回正确的交点，通常是先用HaveIntersectionVague，再用这个检测，此检测仍然有可能返回不存在交点
        public bool GetIntersection(Beam ray, out double t, out Point3D intersection, out Face face) {
            t = 0;
            intersection = null;
            face = null;

            // test boundingbox first
            if (!boundingbox.IsIntersected(ray)) return false;

            bool haveIntersection = false;
            Point3D _inter;
            double _t;
            foreach (var _face in faceList) {
                if (_face.GetIntersection(ray, out _t, out _inter)) {
                    // if the first time or a new intersection which is more close to ray source than the previous one
                    if ((!haveIntersection) || _t < t) {
                        t = _t;
                        intersection = _inter;
                        face = _face;
                    }
                    haveIntersection = true;
                }
            }

            return haveIntersection;
        }


    }
}
