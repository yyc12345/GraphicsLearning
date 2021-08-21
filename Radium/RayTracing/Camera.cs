using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Radium.RayTracing {
    public class Camera {
        public Camera(BinaryReader br) {
            location = new Point3D(br.ReadDouble(), br.ReadDouble(), br.ReadDouble());
            direction = new Vector3D(br.ReadDouble(), br.ReadDouble(), br.ReadDouble());
            up_axis = new Vector3D(br.ReadDouble(), br.ReadDouble(), br.ReadDouble());

            clip_start = br.ReadDouble();
            clip_end = br.ReadDouble();

            viewport_angle_x = br.ReadDouble();
            viewport_angle_y = br.ReadDouble();
        }

        public Point3D location;
        public Vector3D direction;
        public Vector3D up_axis;

        public double clip_start, clip_end;
        public double viewport_angle_x, viewport_angle_y;

    }
}
