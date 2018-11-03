using System;
using System.IO;
using System.Linq;
using GrandTheftAuto.Shared;
using UnityEngine;

namespace GrandTheftAuto.Diagnostics {

    [Serializable]
    public struct TimingsContainer {

        public long totalTicks;
        public TimingSample[] samples;
        public TimingSample[] fastSamples;

        private static Action<TimingsContainer> m_onDump = (t) => { };

        public static Action<TimingsContainer> OnDump {
            get { return m_onDump; }
            set { m_onDump = value; }
        }

        public static string TimingsFolder {
            get {
                var path = string.Format("Timings/", DateTime.Now);
                var folder = Path.GetDirectoryName(path);

                return folder;
            }
        }

        public static void Dump(TimingSample[] samples, TimingSample[] fastSamples) {

            var totalTicks = samples.Max(t => t.ticksTotal);
            var totalTimeSample = new TimingSample() {
                calls = 1,
                label = "Total time",
                ticksTotal = totalTicks,
                ticksSelf = totalTicks,
                stackClass = "FAST"
            };

            var toSave = new TimingsContainer() {
                totalTicks = totalTicks,
                samples = samples,
                fastSamples = fastSamples.Append(totalTimeSample).ToArray()
            };

            var path = GetNewFileName();
            var json = JsonUtility.ToJson(toSave, true);

            File.WriteAllText(path, json);
            OnDump(toSave);
        }

        public static TimingsContainer Load(string path) {
            var json = File.ReadAllText(path);
            var obj = JsonUtility.FromJson<TimingsContainer>(json);

            return obj;
        }

        private static string GetNewFileName() {
            var path = string.Format("Timings/", DateTime.Now);
            var folder = Path.GetDirectoryName(path);

            if(!string.IsNullOrEmpty(folder) && !Directory.Exists(Path.GetDirectoryName(path)))
                Directory.CreateDirectory(folder);

            return path;
        }

    }

}