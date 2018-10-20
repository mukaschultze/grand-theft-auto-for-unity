using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using GrandTheftAuto.Shared;

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

                if(!isRunning) {
                    Log.Warning("Atempt to use Timings.Get() before Timings.Begin(), this is not allowed");
                    return Begin();
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

                // if(Settings.Instance.stackTraceEnabled)
                //     data.stackClass = new StackFrame(1, false).GetMethod().ReflectedType.Name;
                // else
                data.stackClass = string.Empty;

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

            if(isRunning) {
                Log.Warning("Atempt to begin a Timing while another one is already running");
                return null;
            }

            samples.Clear();
            ioWrite.Reset();
            ioRead.Reset();
            overhead.Reset();
            isRunning = true;
            return Get(label);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IDisposable IORead() {

            if(!isRunning) {
                Log.Warning("Atempt to use Timings.IORead() before Timings.Begin(), this is not allowed");
                return Begin();
            }

            ioRead.Start(running.SafePeek().self);
            return ioRead;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IDisposable IOWrite() {

            if(!isRunning) {
                Log.Warning("Atempt to use Timings.IOWrite() before Timings.Begin(), this is not allowed");
                return Begin();
            }

            ioWrite.Start(running.SafePeek().self);
            return ioWrite;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static FastSample Overhead() {
            overhead.Start();
            return overhead;
        }

    }
}