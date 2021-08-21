using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Radium.RayTracing {

    public class Light {
        public enum LightType : byte {
            POINT = 0,
            SUN = 1
        }

        public virtual Vector3D GetDirectionFromPointToSource(Point3D p) {
            throw new NotImplementedException();
        }

        public virtual Color GetColor() {
            throw new NotImplementedException();
        }
    }

    public class PointLight : Light {
        public PointLight(BinaryReader br) {
            position = new Point3D(br.ReadDouble(), br.ReadDouble(), br.ReadDouble());
            color = new Color(br.ReadDouble(), br.ReadDouble(), br.ReadDouble());
        }

        public Point3D position;
        public Color color;

        public override Vector3D GetDirectionFromPointToSource(Point3D p) {
            var cache = position - p;
            cache.SetUnit();
            return cache;
        }

        public override Color GetColor() {
            return new Color(color);
        }
    }


    public class SunLight : Light {
        public SunLight(BinaryReader br) {
            direction = new Vector3D(br.ReadDouble(), br.ReadDouble(), br.ReadDouble());
            color = new Color(br.ReadDouble(), br.ReadDouble(), br.ReadDouble());
        }

        public Vector3D direction;
        public Color color;

        public override Vector3D GetDirectionFromPointToSource(Point3D p) {
            var cache = new Vector3D(direction);
            cache.SetUnit();
            return cache;
        }

        public override Color GetColor() {
            return new Color(color);
        }
    }

}
