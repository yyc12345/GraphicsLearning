using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Radium.Utils {
    public class TracingDebug {
        public TracingDebug(bool global_disable) {
            this.global_disable = global_disable;
            vertices_counter = 1;
            camera = null;
            faceList = new HashSet<RayTracing.Face>();
            fs = new StreamWriter(FileHelper.GetFileName(FileHelper.FileType.TracingDebug), false, Encoding.UTF8);
        }

        public void Close() {
            if (!global_disable) {
                fs.WriteLine($"g faceList");

                var faceCounter = 0;
                foreach(var f in faceList) {
                    fs.WriteLine($"v {f.p1.x} {f.p1.y} {f.p1.z}");
                    fs.WriteLine($"v {f.p2.x} {f.p2.y} {f.p2.z}");
                    fs.WriteLine($"v {f.p3.x} {f.p3.y} {f.p3.z}");
                    fs.WriteLine($"f {vertices_counter + faceCounter} {vertices_counter + faceCounter + 1} {vertices_counter + faceCounter + 2}");

                    faceCounter += 3;
                }
            }

            fs.Close();
            fs.Dispose();
        }

        // always point to the next vertices index(not filled and will be filled)
        public UInt32 vertices_counter;
        public Radium.RayTracing.Point3D camera;
        public StreamWriter fs;
        public HashSet<Radium.RayTracing.Face> faceList;
        public bool global_disable;

        public void NewObject(Radium.RayTracing.Point3D camera_location, UInt32 index) {
            if (global_disable) return;

            fs.WriteLine($"g camera{index}");
            fs.WriteLine($"v {camera_location.x} {camera_location.y} {camera_location.z}");
            vertices_counter = 2;
            camera = new RayTracing.Point3D(camera_location);
        }

        public void NewCameraCasting(Radium.RayTracing.Vector3D direction) {
            if (global_disable) return;

            var new_pos = camera + direction;
            fs.WriteLine($"v {new_pos.x} {new_pos.y} {new_pos.z}");
            fs.WriteLine($"l 1 {vertices_counter}");
            vertices_counter++;
        }

        public void NewVector(Radium.RayTracing.Point3D loc, Radium.RayTracing.Vector3D dir) {
            if (global_disable) return;

            var end = loc + dir;
            fs.WriteLine($"v {loc.x} {loc.y} {loc.z}");
            fs.WriteLine($"v {end.x} {end.y} {end.z}");
            fs.WriteLine($"l {vertices_counter} {vertices_counter + 1}");
            vertices_counter += 2;
        }

        public void NewBeam(Radium.RayTracing.Point3D start, Radium.RayTracing.Point3D end) {
            if (global_disable) return;

            fs.WriteLine($"v {start.x} {start.y} {start.z}");
            fs.WriteLine($"v {end.x} {end.y} {end.z}");
            fs.WriteLine($"l {vertices_counter} {vertices_counter + 1}");
            vertices_counter += 2;
        }

        public void NewFace(Radium.RayTracing.Face f) {
            if (global_disable) return;

            faceList.Add(f);
        }

    }
}
