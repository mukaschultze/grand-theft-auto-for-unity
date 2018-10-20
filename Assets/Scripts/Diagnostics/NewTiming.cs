using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using GrandTheftAuto.Diagnostics;

namespace GrandTheftAuto.Diagnostics {

    [Serializable]
    public struct TimingSample {
        public string label;
        public string stackClass;
        public int calls;
        public long ticksSelf;
        public long ticksTotal;
    }

    public class Timing : IDisposable {

        private const int STACK_CAPACITY = 256;
        private const int DICTIONARY_CAPACITY = 128;

        private static Stopwatch overhead = new Stopwatch();
        private static Stack<Timing> waiting = new Stack<Timing>(STACK_CAPACITY);
        private static Stack<Timing> running = new Stack<Timing>(STACK_CAPACITY);
        private static Dictionary<string, TimingSample> samples = new Dictionary<string, TimingSample>(DICTIONARY_CAPACITY);

        private string label;
        private Stopwatch self;
        private Stopwatch total;

        static Timing() {
            for(var i = 0; i < STACK_CAPACITY; i++)
                waiting.Push(new Timing());
        }

        private Timing() {
            self = new Stopwatch();
            total = new Stopwatch();
        }

        public static Timing Get(string label) {
            overhead.Start();

            var parent = running.SafePeek();
            var timing = StackUtility.MoveStack(waiting, running);

            if(timing == null)
                Log.Error("Not enough timing samples ({0})", STACK_CAPACITY);

            if(parent != null)
                parent.self.Stop();

            timing.label = label;
            timing.self.Start();
            timing.total.Start();

            overhead.Stop();
            return timing;
        }

        public void Dispose() {
            overhead.Start();

            var timing = StackUtility.MoveStack(running, waiting);
            var parent = running.SafePeek();

            if(timing != this)
                Log.Error("Timings out of sync");

            TimingSample data;

            samples.TryGetValue(label, out data);
            data.label = label;
            data.calls++;
            data.ticksSelf += self.ElapsedTicks;
            data.ticksTotal += total.ElapsedTicks;
            samples[label] = data;

            self.Reset();
            total.Reset();

            if(parent != null)
                parent.self.Start();
            else {
                TimingsContainer.Dump(overhead.ElapsedTicks, samples.Select(kvp => kvp.Value).ToArray());
                overhead.Reset();
            }

            overhead.Stop();
        }

        public static Timing Begin(string label = "main") {
            overhead.Reset();
            return Get("main");
        }

        public static Timing IO() {
            return Get("io");
        }

        public static Timing IO(bool read) {
            return Get(read? "io/read": "io/write");
        }

        public static Timing Pause() {
            return Get("pause");
        }

    }
}