using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using GrandTheftAuto.Diagnostics;

namespace GrandTheftAuto.Diagnostics {

    [Serializable]
    public struct TimingData {
        public string name;
        public string stackClass;
        public int calls;
        public long ticksSelf;
        public long ticksTotal;
    }

    public class Timing : IDisposable {

        private const int STACK_CAPACITY = 256;
        private const int DICTIONARY_CAPACITY = 128;

        private static Stack<Timing> waiting = new Stack<Timing>(STACK_CAPACITY);
        private static Stack<Timing> running = new Stack<Timing>(STACK_CAPACITY);
        private static Dictionary<string, TimingData> all = new Dictionary<string, TimingData>(DICTIONARY_CAPACITY);
        private static Stopwatch overhead = new Stopwatch();

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

            var parent = SafePeek(running);
            var timing = MoveStack(waiting, running);

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

            var timing = MoveStack(running, waiting);
            var parent = SafePeek(running);

            if(timing != this)
                Log.Error("Timings out of sync");

            TimingData data;

            all.TryGetValue(label, out data);
            data.name = label;
            data.calls++;
            data.ticksSelf += self.ElapsedTicks;
            data.ticksTotal += total.ElapsedTicks;
            all[label] = data;

            self.Reset();
            total.Reset();

            if(parent != null)
                parent.self.Start();
            else {
                TimingSave.Dump(overhead.ElapsedTicks, all.Select(kvp => kvp.Value).ToArray());
                overhead.Reset();
            }

            overhead.Stop();
        }

        private static T MoveStack<T>(Stack<T> from, Stack<T> to) {
            var obj = SafePop(from);
            to.Push(obj);
            return obj;
        }

        private static T SafePop<T>(Stack<T> stack) {
            if(stack.Count > 0)
                return stack.Pop();
            else
                return default(T);
        }

        private static T SafePeek<T>(Stack<T> stack) {
            if(stack.Count > 0)
                return stack.Peek();
            else
                return default(T);
        }

        public static Timing Begin() {
            overhead.Reset();
            return Get("main");
        }

        public static Timing IO() {
            return Get("io");
        }

        public static Timing Pause() {
            return Get("pause");
        }

    }
}