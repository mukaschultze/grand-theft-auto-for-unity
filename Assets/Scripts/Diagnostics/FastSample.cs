using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace GrandTheftAuto.Diagnostics {
    public class FastSample : IDisposable {

        public int calls;
        private Stopwatch stopwatch;
        private Stopwatch toStop;

        public FastSample() {
            stopwatch = new Stopwatch();
        }

        public void Reset() {
            calls = 0;
            stopwatch.Reset();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Start(Stopwatch toStop = null) {
            calls++;
            stopwatch.Start();
            this.toStop = toStop;

            if(toStop != null)
                toStop.Stop();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose() {
            stopwatch.Stop();

            if(toStop != null)
                toStop.Start();
        }

        public TimingSample ToTimingSample(string label) {
            return new TimingSample() {
                calls = calls,
                    label = label,
                    ticksTotal = stopwatch.ElapsedTicks,
                    ticksSelf = stopwatch.ElapsedTicks,
                    stackClass = "FAST"
            };
        }

    }
}