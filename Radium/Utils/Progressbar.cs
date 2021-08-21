using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Radium.Utils {
    public class Progressbar {

        private int all, now;
        private static readonly int progressbar_span = 2;
        private static readonly int progressbar_count = 100 / progressbar_span;

        public Progressbar(int _all) {
            all = _all;
            now = 0;

            Console.Write((new String('=', progressbar_count)) + " 0.00%");
        }

        public void Step() {
            now++;
            if (now > all) now = all;

            var percentage = now / all * 100;
            var percentage_bar = now / all * progressbar_count;

            Console.Write("\r" + (new String('#', percentage_bar)) + (new String('=', (progressbar_count - percentage_bar))) + " " + percentage.ToString("##0.00%"));
        }

        public void Finish() {
            Console.Write("\r" + (new String('#', progressbar_count)) + " 100.00%");
            Console.WriteLine();
        }

    }
}
