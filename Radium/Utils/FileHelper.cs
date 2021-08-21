using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Radium.Utils {
    public class FileHelper {

        //public static readonly Encoding UTF8WithoutBOM = new UTF8Encoding(false);

        public enum FileType {
            Image,
            TracingDebug
        }

        public static string GetFileName(FileType type) {
            string suffix = "";
            string foldername = "";
            switch (type) {
                case FileType.Image:
                    suffix = "png";
                    foldername = "imgs";
                    break;
                case FileType.TracingDebug:
                    suffix = "obj";
                    foldername = "objs";
                    break;
                default:
                    throw new Exception("Undefined type of FileType!");
            }

            // ensure the existance of folder
            if (!Directory.Exists($"./{foldername}"))
                Directory.CreateDirectory($"./{foldername}");

            int counter = 1;
            var datetime = DateTime.Now;
            string filename;
            while (true) {
                filename = $"./{foldername}/{datetime.ToString("yyyyMMddHHmmss")}-{counter.ToString().PadLeft(2, '0')}.{suffix}";
                if (!File.Exists(filename)) break;
            }

            Console.WriteLine($"Alloc file: {filename}");
            return filename;
        }

    }
}
