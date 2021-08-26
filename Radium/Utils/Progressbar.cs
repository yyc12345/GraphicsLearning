using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Radium.Utils {
    public class Progressbar {

        private Stopwatch stopwatch;
        private bool is_over;

        public Progressbar() {
            is_over = false;
            stopwatch = new Stopwatch();
            stopwatch.Start();
        }

        public void Finish() {
            if (!is_over) {
                is_over = true;
                stopwatch.Stop();
                Console.WriteLine($"Time Elapsed: {stopwatch.Elapsed}");
            }
        }

    }
}
