using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using GrandTheftAuto.Shared;
using UnityEngine;

namespace GrandTheftAuto.Diagnostics {

    public struct TimingSample {

        private long durationTicks;

        public int Calls { get; set; }
        public string Name { get; set; }
        public string StackClass { get; set; }
        public Stopwatch Stopwatch { get; set; }
        public TimeSpan Duration {
            get {
                if(Stopwatch == null)
                    return new TimeSpan(durationTicks);
                else
                    return Stopwatch.Elapsed;
            }
            set { durationTicks = value.Ticks; }
        }

    }

    public class TimingGroup {

        public TimeSpan TotalTime { get; private set; }
        public TimeSpan OverheadTime { get; private set; }
        public TimingSample[] Timings { get; private set; }
        public static Action<TimingGroup> OnNewTimingCreated { get; set; }

        public TimingGroup(TimingSample[] timings, Stopwatch overhead) {
            TotalTime = new TimeSpan();
            OverheadTime = overhead.Elapsed;
            Timings = timings;

            foreach(var t in timings)
                TotalTime = TotalTime.Add(t.Duration);
        }

        public TimingGroup(string path) {
            using(var stream = new FileStream(path, FileMode.Open))
            using(var reader = new BinaryReader(stream)) {
                TotalTime = new TimeSpan(reader.ReadInt64());
                OverheadTime = new TimeSpan(reader.ReadInt64());
                Timings = new TimingSample[reader.ReadInt32()];

                for(var i = 0; i < Timings.Length; i++) {
                    Timings[i].Name = reader.ReadString();
                    Timings[i].StackClass = reader.ReadString();
                    Timings[i].Calls = reader.ReadInt32();
                    Timings[i].Duration = new TimeSpan(reader.ReadInt64());
                }
            }
        }

        public void Save(string path) {
            using(var stream = new FileStream(path, FileMode.Create))
            using(var writer = new BinaryWriter(stream)) {
                writer.Write(TotalTime.Ticks);
                writer.Write(OverheadTime.Ticks);
                writer.Write(Timings.Length);

                for(var i = 0; i < Timings.Length; i++) {
                    writer.Write(Timings[i].Name);
                    writer.Write(Timings[i].StackClass);
                    writer.Write(Timings[i].Calls);
                    writer.Write(Timings[i].Duration.Ticks);
                }
            }
        }

    }

    public class Timing : IDisposable {

        private Timing parent;
        private TimingSample info;

        private static Timing current;
        private static readonly Stopwatch overheadTime = new Stopwatch();
        private static readonly Dictionary<string, TimingSample> timings = new Dictionary<string, TimingSample>(25);

        public static bool Running { get { return current != null; } }
        public static string TimingsSaveFolder {
            get {
                var folder = Settings.Instance.timingsFolder;
                if(!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);
                return folder;
            }
        }

        public Timing(string name) {
            overheadTime.Start();

            parent = current;
            current = this;

            var stackClass = new StackFrame(1, false).GetMethod().ReflectedType.Name;
            var key = string.Format("{0}_{1}", name, stackClass);

            if(!timings.TryGetValue(key, out info))
                timings.Add(key, info = new TimingSample() {
                    Name = name,
                        StackClass = stackClass,
                        Stopwatch = new Stopwatch()
                });

            info.Calls++;

            if(parent != null)
                parent.info.Stopwatch.Stop();

            info.Stopwatch.Start();
            timings[key] = info;
            overheadTime.Stop();
        }

        public void Dispose() {
            overheadTime.Start();

            info.Stopwatch.Stop();
            current = parent;

            if(current != null)
                current.info.Stopwatch.Start();
            else
                EndAll();

            overheadTime.Stop();
        }

        private static void EndAll() {
            if(current != null) {
                Log.Warning("There's a stopwatch running, make sure to dispose all the timers");
                current = null;
            }

            var group = new TimingGroup(timings.Values.ToArray(), overheadTime);
            var path = GetUniqueTimingFileName();
            group.Save(path);

            if(TimingGroup.OnNewTimingCreated != null)
                TimingGroup.OnNewTimingCreated(new TimingGroup(path));

            foreach(var timing in timings.Values) {
                if(timing.Stopwatch.IsRunning)
                    Log.Warning("Stopwatch for \"{0}\" is still running, maybe you didn't disposed the timer?", timing);

                timing.Stopwatch.Reset();
            }

            timings.Clear();
            overheadTime.Reset();
        }

        private static string GetUniqueTimingFileName() {
            return Path.Combine(TimingsSaveFolder, string.Format("Timing_{0:yyyy_MM_dd_HH_mm_ss}.timing", DateTime.Now));
        }

    }

}