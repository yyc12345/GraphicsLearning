using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Radium {
    class Program {
        static void Main(string[] args) {
            if (args.Length != 2) {
                Console.WriteLine("Wrong parameter!");
                return;
            }
            if (!System.IO.File.Exists(args[0])) {
                Console.WriteLine("Invalid File!");
                return;
            }
            UInt32 imageWidth;
            if (!UInt32.TryParse(args[1], out imageWidth)) {
                Console.WriteLine("Invalid image size!");
                return;
            }

            var progress = new Radium.Utils.Progressbar();
            var scene = new RayTracing.Scene(args[0]);
            scene.Render(imageWidth);
            progress.Finish();

            Console.WriteLine("OK!");
            return;
        }
    }
}
