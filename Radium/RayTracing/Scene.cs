using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Radium.RayTracing {
    public class Scene {
        public Scene(string filename) {
            var fs = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read);
            var br = new BinaryReader(fs);

            if (br.ReadUInt32() != MAGIC_WORDS) throw new Exception("Not matched magic words!");
            if (br.ReadUInt32() != RS_CURRENT_VERSION) throw new Exception("Not matched RS file version!");

            // read camera
            var cameraCount = br.ReadUInt32();
            cameraList = new Camera[cameraCount];
            for (UInt32 i = 0; i < cameraCount; i++) {
                cameraList[i] = new Camera(br);
            }

            // read light
            var lightCount = br.ReadUInt32();
            lightList = new Light[lightCount];
            for (UInt32 i = 0; i < lightCount; i++) {
                Light.LightType type = (Light.LightType)br.ReadByte();
                switch (type) {
                    case Light.LightType.POINT:
                        lightList[i] = new PointLight(br);
                        break;
                    case Light.LightType.SUN:
                        lightList[i] = new SunLight(br);
                        break;
                }
            }

            // read mesh obj
            var meshObjCount = br.ReadUInt32();
            meshObjectList = new MeshObject[meshObjCount];
            for (UInt32 i = 0; i < meshObjCount; i++) {
                meshObjectList[i] = new MeshObject(br);
            }

            // read material
            var materialCount = br.ReadUInt32();
            materialList = new Material[materialCount];
            for (UInt32 i = 0; i < materialCount; i++) {
                materialList[i] = new Material(br);
            }

            // read texture
            var textureCount = br.ReadUInt32();
            textureList = new Texture[textureCount];
            for (UInt32 i = 0; i < textureCount; i++) {
                textureList[i] = new Texture(br);
            }

            // fill mesh obj data
            foreach (var obj in meshObjectList) {
                obj.FillData(materialList);
            }

            // fill texture data
            foreach (var mat in materialList) {
                mat.FillData(textureList);
            }

            br.Close();
            br.Dispose();
        }

        private const UInt32 MAGIC_WORDS = 61;
        private const UInt32 RS_CURRENT_VERSION = 0;

        private MeshObject[] meshObjectList;
        private Camera[] cameraList;
        private Light[] lightList;
        private Material[] materialList;
        private Texture[] textureList;

        public void Render(UInt32 imgWidth) {
#if DEBUG
            var debug = new Radium.Utils.TracingDebug(false);
            UInt32 debug_counter = 0;
#endif

            foreach (var camera in cameraList) {
                // calc unit vector in x y axis
                Vector3D vx = camera.direction ^ camera.up_axis;
                Vector3D vy = new Vector3D(camera.up_axis);

                Vector3D dir = new Vector3D(camera.direction);

                // calc center dir len and imgHeight
                var halfWidth = imgWidth / 2;
                var centerDirLen = halfWidth / Math.Tan(camera.viewport_angle_x / 2);
                UInt32 halfHeight = (UInt32)(Math.Tan(camera.viewport_angle_y / 2) * centerDirLen);
                var imgHeight = 2 * halfHeight;
                dir.SetLen(centerDirLen);
                vx.SetUnit();
                vy.SetUnit();

#if DEBUG
                debug.NewObject(camera.location, debug_counter);
                debug_counter++;
#endif

                // render
                var bitmap = new Radium.Utils.Bitmap((int)imgWidth, (int)imgHeight);
                for (UInt32 y = 0; y < imgHeight; y++) {
                    for (UInt32 x = 0; x < imgWidth; x++) {
                        Vector3D tvx = new Vector3D(vx);
                        tvx.SetLen((Int32)x - (Int32)halfWidth);
                        Vector3D tvy = new Vector3D(vy);
                        tvy.SetLen((Int32)halfHeight - (Int32)y);
                        
                        Vector3D direction = dir + tvx + tvy;
                        direction.SetUnit();

                        var ray = new Beam(direction, camera.location);
#if DEBUG
                        //debug.NewCameraCasting(direction);
                        var color = TracingOneRay(ray, 0, 1.0f, debug, x % 10 == 0 && y % 10 == 0);
#else
                        var color = TracingOneRay(ray, 0, 1.0f);
#endif
                        color = color * 255;
                        bitmap.SetPixel((int)x, (int)y, new Utils.Color(color));
                    }
                }
                bitmap.Close();
            }

#if DEBUG
            debug.Close();
#endif
        }

#if DEBUG
        public Color TracingOneRay(Beam ray, UInt32 depth, float weight, Radium.Utils.TracingDebug debug, bool need_draw) {
#else
        public Color TracingOneRay(Beam ray, UInt32 depth, float weight) {
#endif
            double intersection_t;
            Point3D intersection_point;
            Face intersection_face;

            if (!GetIntersection(ray, out intersection_t, out intersection_point, out intersection_face)) {
                return new Color(UtilFunc.DEFAULT_ENVIRONMENT);
            }

#if DEBUG
            if (need_draw) {
                debug.NewBeam(ray.source, intersection_point);
                debug.NewFace(intersection_face);
            }
#endif

            // calc local color
#if DEBUG
            var local_color = intersection_face.GetLocalColor(ray, intersection_point, lightList, debug, need_draw);
#else
            var local_color = intersection_face.GetLocalColor(ray, intersection_point, lightList);
#endif

            // combine color
            var final_color = new Color(local_color);

            // clamp
            if (final_color.r > 1) final_color.r = 1;
            if (final_color.g > 1) final_color.g = 1;
            if (final_color.b > 1) final_color.b = 1;
            if (final_color.r < 0) final_color.r = 0;
            if (final_color.g < 0) final_color.g = 0;
            if (final_color.b < 0) final_color.b = 0;
            return final_color;
        }

        public bool GetIntersection(Beam ray, out double t, out Point3D intersection, out Face face) {
            t = 0;
            intersection = null;
            face = null;
            bool haveIntersection = false;

            Point3D _intersection;
            double _t;
            Face _face;
            foreach (var obj in meshObjectList) {
                if (obj.GetIntersection(ray, out _t, out _intersection, out _face)) {
                    if (!haveIntersection || _t < t) {
                        t = _t;
                        intersection = _intersection;
                        face = _face;
                    }
                    haveIntersection = true;
                }
            }

            return haveIntersection;
        }
    }
}
