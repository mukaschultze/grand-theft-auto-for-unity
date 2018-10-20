using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;

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

        private static FastSample ioRead = new FastSample();
        private static FastSample ioWrite = new FastSample();
        private static FastSample overhead = new FastSample();

        private static bool isRunning;
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Timing Get(string label) {
            using(Overhead()) {

                if(isRunning) {
                    Log.Warning("Atempt to usa Timings.Get() before Timings.Begin(), this is not allowed");
                    return null;
                }

                var parent = running.SafePeek();
                var timing = StackUtility.MoveStack(waiting, running);

                if(timing == null)
                    Log.Error("Not enough timing samples ({0})", STACK_CAPACITY);

                if(parent != null)
                    parent.self.Stop();

                timing.label = label;
                timing.self.Start();
                timing.total.Start();

                return timing;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose() {
            using(Overhead()) {
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
                data.stackClass = new StackFrame(1, false).GetMethod().ReflectedType.Name;
                samples[label] = data;

                self.Reset();
                total.Reset();

                if(parent != null)
                    parent.self.Start();
                else {
                    var fastSamples = new [] {
                        overhead.ToTimingSample("Overhead"),
                        ioWrite.ToTimingSample("IO Write"),
                        ioRead.ToTimingSample("IO Read")
                    };

                    TimingsContainer.Dump(samples.Select(kvp => kvp.Value).ToArray(), fastSamples);
                    overhead.Reset();
                    isRunning = false;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Timing Begin(string label = "main") {
            ioWrite.Reset();
            ioRead.Reset();
            overhead.Reset();
            isRunning = true;
            return Get(label);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IDisposable IORead() {

            if(isRunning) {
                Log.Warning("Atempt to usa Timings.IORead() before Timings.Begin(), this is not allowed");
                return null;
            }

            ioRead.Start(running.SafePeek().self);
            return ioRead;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IDisposable IOWrite() {

            if(isRunning) {
                Log.Warning("Atempt to usa Timings.IOWrite() before Timings.Begin(), this is not allowed");
                return null;
            }

            ioWrite.Start(running.SafePeek().self);
            return ioWrite;
        }

        private static FastSample Overhead() {
            overhead.Start();
            return overhead;
        }

    }
}