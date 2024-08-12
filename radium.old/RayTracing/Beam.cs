using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Radium.RayTracing {
    public class Beam {
        public Beam(Vector3D v, Point3D p) {
            direction = new Vector3D(v);
            source = new Point3D(p);
        }

        public Vector3D direction;
        public Point3D source;
    }
}
