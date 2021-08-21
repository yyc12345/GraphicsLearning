using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Radium.RayTracing {
    public class Material {
        public Material() {
            // construct default material
            ambient = new Color(1, 1, 1);
            diffuse = new Color(1, 1, 1);
            specular = new Color(1, 1, 1);

            specularN = 64.0;
        }

        public Material(BinaryReader br) {
            ambient = new Color(br.ReadDouble(), br.ReadDouble(), br.ReadDouble());
            diffuse = new Color(br.ReadDouble(), br.ReadDouble(), br.ReadDouble());
            specular = new Color(br.ReadDouble(), br.ReadDouble(), br.ReadDouble());

            specularN = br.ReadDouble();
        }

        public Color ambient, diffuse, specular;
        public double specularN;

    }
}
