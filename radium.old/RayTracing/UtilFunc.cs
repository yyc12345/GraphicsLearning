using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Radium.RayTracing {
    public static class UtilFunc {

        public static readonly UInt32 MAX_DEPTH = 2;

        public static readonly double MIN_WEIGHT = 0.05;

        public static readonly double TOLERANCE = 1e-7;

        public static readonly Material DEFAULT_MATERIAL = new Material();

        public static readonly Color DEFAULT_AMBIENT = new Color(0.2, 0.2, 0.2);

        public static readonly Skybox DEFAULT_ENVIRONMENT = new Skybox();

        public static bool CloseBy(double num, double expected) {
            return Math.Abs(num - expected) < TOLERANCE;
        }

        public static bool ToBoolean(this byte bt) {
            return (bt != 0);
        }
    }
}
