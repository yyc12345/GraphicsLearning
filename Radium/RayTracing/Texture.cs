using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Radium.RayTracing {
    public class Texture {

        public struct TextureRGB {
            public double r;
            public double g;
            public double b;
        }

        public Texture(BinaryReader br) {
            width = br.ReadUInt32();
            height = br.ReadUInt32();

            pixels = new TextureRGB[width, height];

            for(UInt32 y = 0; y < height; y++) {
                for (UInt32 x = 0; x < width; x++) {
                    pixels[x, y].r = br.ReadDouble();
                    pixels[x, y].g = br.ReadDouble();
                    pixels[x, y].b = br.ReadDouble();
                }
            }
        }

        public TextureRGB[,] pixels;
        public UInt32 width, height;

        public Vector3D GetPixel(Point2D uv) {
            // due to blender image.pixels' storage mode, we don't need conv uv to texture xy, because the stored data has been uv coordinate
            double x = uv.x - Math.Truncate(uv.x);
            double y = uv.y - Math.Truncate(uv.y);

            x *= width - 1;
            y *= height - 1;

            // clamp
            if (x < 0) x = 0;
            if (x > width - 1) x = width - 1;
            if (y < 0) y = 0;
            if (y > height - 1) y = height - 1;

            UInt32 basex = (UInt32)x, basey = (UInt32)y;
            if (basex == width - 1 || basey == height - 1) return GetPixel(basex, basey);

            var x_percentage = x - (double)basex;
            var y_percentage = y - (double)basey;

            var midCache1 = GetPixel(basex, basey) * (1 - x_percentage) + GetPixel(basex + 1, basey) * x_percentage;
            var midCache2 = GetPixel(basex, basey + 1) * (1 - x_percentage) + GetPixel(basex + 1, basey + 1) * x_percentage;

            return (midCache1 * (1 - y_percentage) + midCache2 * y_percentage);
        }

        public Vector3D GetPixel(UInt32 x, UInt32 y) {
            var col = pixels[x, y];
            return new Vector3D(col.r, col.g, col.b);
        }
    }
}
