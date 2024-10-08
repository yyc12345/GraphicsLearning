﻿using System;
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
                obj.FillData(this);
            }

            // fill texture data
            foreach (var mat in materialList) {
                mat.FillData(this);
            }

            br.Close();
            br.Dispose();
        }

        private const UInt32 MAGIC_WORDS = 61;
        private const UInt32 RS_CURRENT_VERSION = 0;

        public MeshObject[] meshObjectList;
        public Camera[] cameraList;
        public Light[] lightList;
        public Material[] materialList;
        public Texture[] textureList;

        public void Render(UInt32 imgWidth) {
#if DEBUG
            var debug = new Radium.Utils.TracingDebug(true);
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
                        var color = TracingOneRay(ray, 0, 1.0, debug, x % 5 == 0 && y % 5 == 0);
#else
                        var color = TracingOneRay(ray, 0, 1.0);
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
        public Color TracingOneRay(Beam ray, UInt32 depth, double weight, Radium.Utils.TracingDebug debug, bool need_draw) {
#else
        public Color TracingOneRay(Beam ray, UInt32 depth, double weight) {
#endif
            // test depth and weight first
            if (depth > UtilFunc.MAX_DEPTH || weight < UtilFunc.MIN_WEIGHT)
                return new Color(0, 0, 0);

            // test intersection
            double intersection_t;
            Point3D intersection_point;
            Face intersection_face;

            if (!GetIntersection(ray, out intersection_t, out intersection_point, out intersection_face)) {
                return UtilFunc.DEFAULT_ENVIRONMENT.GetColor(ray.direction);
            }

#if DEBUG
            //if (need_draw) {
            //    debug.NewBeam(ray.source, intersection_point);
            //    //debug.NewFace(intersection_face);
            //}
#endif

            // calc face weight and normal
            intersection_face.GetWeight(intersection_point, out double face_w1, out double face_w2, out double face_w3);
#if DEBUG
            var normal = intersection_face.GetInternalPointNormal(intersection_point, face_w1, face_w2, face_w3, debug, need_draw);
#else
            var normal = intersection_face.GetInternalPointNormal(intersection_point, face_w1, face_w2, face_w3);
#endif

            // calc local color
#if DEBUG
            var local_color = GetLocalColor(intersection_face, ray, intersection_point, normal, face_w1, face_w2, face_w3, debug, need_draw);
#else
            var local_color = GetLocalColor(intersection_face, ray, intersection_point, normal, face_w1, face_w2, face_w3);
#endif

            // camera view direction
            var v = -ray.direction;
            // calc kr
            Color rColor = null;
            if (v * normal < UtilFunc.TOLERANCE) rColor = new Color(0.0, 0.0, 0.0);
            else {
                var direction = normal * 2 * (normal * v) - v;
                direction.SetUnit();
                var rray = new Beam(direction, intersection_point);
#if DEBUG
                rColor = TracingOneRay(rray, depth + 1, weight * intersection_face.material.kr, debug, need_draw);
#else
                rColor = TracingOneRay(rray, depth + 1, weight * intersection_face.material.kr);
#endif
            }

            // calc kt
            Color tColor = null;
            if (intersection_face.material.ior <= 1 + UtilFunc.TOLERANCE) tColor = new Color(0.0, 0.0, 0.0);
            else {
                double cos1, cos2;
                if (v * normal < UtilFunc.TOLERANCE) {
                    // inside of object
                    normal = -normal;
                    cos1 = normal * v;
                    double tmp = 1 - Math.Pow(intersection_face.material.ior, 2) * (1 - Math.Pow(cos1, 2));
                    if (tmp < 0) tColor = new Color(0.0, 0.0, 0.0);
                    else {
                        cos2 = Math.Sqrt(tmp);
                        var direction = -v * intersection_face.material.ior - normal * (cos2 - intersection_face.material.ior * cos1);
                        direction.SetUnit();
                        var tray = new Beam(direction, intersection_point);
#if DEBUG
                        tColor = TracingOneRay(tray, depth + 1, weight * intersection_face.material.kt, debug, need_draw);
#else
                        tColor = TracingOneRay(tray, depth + 1, weight * intersection_face.material.kt);
#endif
                    }
                } else {
                    // outside of object
                    cos1 = normal * v;
                    double eta = 1 / intersection_face.material.ior;
                    double tmp = 1 - Math.Pow(eta, 2) * (1 - Math.Pow(cos1, 2));
                    cos2 = Math.Sqrt(tmp);
                    var _direction = -v * eta - normal * (cos2 - eta * cos1);
                    _direction.SetUnit();
                    var _tray = new Beam(_direction, intersection_point);
#if DEBUG
                    tColor = TracingOneRay(_tray, depth + 1, weight * intersection_face.material.kt, debug, need_draw);
#else
                    tColor = TracingOneRay(_tray, depth + 1, weight * intersection_face.material.kt);
#endif
                }
            }

            // combine color
            var final_color = (local_color + rColor + tColor) * weight;

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


#if DEBUG
        public Color GetLocalColor(Face f, Beam ray, Point3D p, Vector3D normal, double w1, double w2, double w3, Radium.Utils.TracingDebug debug, bool need_draw) {
#else
        public Color GetLocalColor(Face f, Beam ray, Point3D p, Vector3D normal, double w1, double w2, double w3) {
#endif
            // ambient
            var result = f.material.ambient * UtilFunc.DEFAULT_AMBIENT;
            // diffuse color
            var diffuse_color = f.GetDiffuse(p, w1, w2, w3);

#if DEBUG
            if (need_draw)
                debug.NewVector(p, normal);
#endif

            // for each light, calc diffuse and specular
            foreach (var light in lightList) {
                var L = light.GetDirectionFromPointToSource(p);

                // shadow confirm
                var newray = new Beam(L, p);
                var in_shadow = false;
                foreach (var obj in meshObjectList) {
                    if (obj.HaveIntersection(newray, light.GetDistance(p))) {
                        in_shadow = true;
                        break;
                    }
                }
                if (in_shadow) continue;    // if in shadow, skip this light

                // calc diffuse
                var V = -ray.direction;
                var LN = L * normal;
                if (LN < 0) continue;
                result = result + (light.GetColor(p) * diffuse_color * LN);

                V.SetUnit();
                var H = L + V;
                H.SetUnit();

                // calc specular
                var HN = H * normal;
                if (HN < 0) continue;
                result = result + (light.GetColor(p) * f.material.specular *
                    Math.Pow(HN, f.material.specularN));
            }

            return result;
        }


    }
}
