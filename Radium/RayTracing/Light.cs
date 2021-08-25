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

        public virtual double GetDistance(Point3D p) {
            throw new NotImplementedException();
        }

        public virtual Color GetColor(Point3D target) {
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

        public override double GetDistance(Point3D p) {
            var cache = position - p;
            return cache.GetLen();
        }

        public override Color GetColor(Point3D target) {
            var distance = position - target;
            return color / Math.Pow(distance.GetLen(), 2);
            //return new Color(color);
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
            return -direction;
        }

        public override double GetDistance(Point3D p) {
            return Double.PositiveInfinity;
        }

        public override Color GetColor(Point3D target) {
            return new Color(color);
        }
    }

}
