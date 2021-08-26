using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Radium.RayTracing {
    public class Skybox {
        private static Color sky_top_color = new Color(0.65, 0.84, 0.95);
        private static Color sky_horizon_color = new Color(0.84, 0.92, 0.98);
        private static Color ground_bottom_color = new Color(0.16, 0.18, 0.21);
        private static Color ground_horizon_color = new Color(0.42, 0.40, 0.37);
        private static Vector3D VECTOR_Z = new Vector3D(0, 0, 1);

        private static double sky_curve = 0.2;
        private static double sky_curve_unit = 1.0 / 0.2;
        private static double ground_curve = 0.2;
        private static double ground_curve_unit = 1.0 / 0.2;

        public Color GetColor(Vector3D v) {
            var _v = new Vector3D(v);
            _v.SetUnit();
            var tmp = _v * VECTOR_Z;

            if (tmp >= 0) {
                // sky
                if (tmp >= sky_curve) return new Color(sky_top_color);
                else return sky_top_color * (tmp * sky_curve_unit) + sky_horizon_color * (1 - tmp * sky_curve_unit);
            } else {
                // ground
                tmp = -tmp;
                if (tmp >= ground_curve) return new Color(ground_bottom_color);
                else return ground_bottom_color * (tmp * ground_curve_unit) + ground_horizon_color * (1 - tmp * ground_curve_unit);
            }
        }

    }
}
