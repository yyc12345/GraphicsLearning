using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Radium.Utils {

    public struct Color {
        public Color(byte r, byte g, byte b) {
            R = r;
            G = g;
            B = b;
        }
        public Color(Radium.RayTracing.Color c) {
            R = (byte)c.r;
            G = (byte)c.g;
            B = (byte)c.b;
        }
        public byte R;
        public byte G;
        public byte B;
    }

    public class Bitmap {

        public Bitmap(int width, int height) {
            // init variables
            isOpen = true;
            fileUrl = FileHelper.GetFileName(FileHelper.FileType.Image);
            image = new System.Drawing.Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            realImage = image.LockBits(
                new System.Drawing.Rectangle(0, 0, image.Width, image.Height),
                System.Drawing.Imaging.ImageLockMode.ReadWrite,
                System.Drawing.Imaging.PixelFormat.Format24bppRgb);
        }

        bool isOpen;
        string fileUrl;
        System.Drawing.Bitmap image;
        System.Drawing.Imaging.BitmapData realImage;
        const int PIXEL_SIZE = 3;
        public int Width { get { return image.Width; } }
        public int Height { get { return image.Height; } }

        public void Close() {
            if (!isOpen) throw new Exception("Image is closed.");

            //unlock
            image.UnlockBits(realImage);
            //save
            image.Save(fileUrl, System.Drawing.Imaging.ImageFormat.Png);
            //release
            image.Dispose();
            //mark
            isOpen = false;
        }

        public void SetPixel(int x, int y, Color color) {
            int offset = y * realImage.Stride + x * PIXEL_SIZE;
            //RGB -> BGR
            System.Runtime.InteropServices.Marshal.WriteByte(realImage.Scan0, offset, color.B);
            System.Runtime.InteropServices.Marshal.WriteByte(realImage.Scan0, offset + 1, color.G);
            System.Runtime.InteropServices.Marshal.WriteByte(realImage.Scan0, offset + 2, color.R);
        }
    }



}
