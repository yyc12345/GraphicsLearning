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

            kr = 0.0;
            kt = 0.0;

            use_base_color_texture = false;
            index_base_color_texture = 0;
            base_color_texture = null;
            use_normalmap_texture = false;
            index_normalmap_texture = 0;
            normalmap_texture = null;
        }

        public Material(BinaryReader br) {
            ambient = new Color(br.ReadDouble(), br.ReadDouble(), br.ReadDouble());
            diffuse = new Color(br.ReadDouble(), br.ReadDouble(), br.ReadDouble());
            specular = new Color(br.ReadDouble(), br.ReadDouble(), br.ReadDouble());

            specularN = br.ReadDouble();

            kr = br.ReadDouble();
            kt = br.ReadDouble();

            use_base_color_texture = br.ReadByte().ToBoolean();
            index_base_color_texture = br.ReadUInt32();
            //base_color_texture = null;
            use_normalmap_texture = br.ReadByte().ToBoolean();
            index_normalmap_texture = br.ReadUInt32();
            //normalmap_texture = null;
        }

        public Color ambient, diffuse, specular;
        public double specularN, kr, kt;
        public bool use_base_color_texture, use_normalmap_texture;
        public UInt32 index_base_color_texture, index_normalmap_texture;
        public Texture base_color_texture, normalmap_texture;   // use this to confirm whis material whether use texture, not use `use_` variable, it just the field read from file

        public void FillData(Texture[] textureList) {
            if (use_base_color_texture) base_color_texture = textureList[index_base_color_texture];
            else base_color_texture = null;

            if (use_normalmap_texture) normalmap_texture = textureList[index_normalmap_texture];
            else normalmap_texture = null;
        }
    }
}
