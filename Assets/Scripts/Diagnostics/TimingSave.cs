using System;
using System.IO;
using System.Linq;
using GrandTheftAuto.Shared;
using UnityEngine;

namespace GrandTheftAuto.Diagnostics {

    [Serializable]
    public struct TimingsContainer {
        public long totalTicks;
        public long overheadTicks;
        public TimingSample[] samples;

        private static Action<TimingsContainer> m_onDump = (t) => { };

        public static Action<TimingsContainer> OnDump {
            get { return m_onDump; }
            set { m_onDump = value; }
        }

        public static string TimingsFolder {
            get {
                var path = string.Format(Settings.Instance.timingsPath, DateTime.Now);
                var folder = Path.GetDirectoryName(path);

                return folder;
            }
        }

        public static void Dump(long overheadTicks, TimingSample[] samples) {

            var toSave = new TimingsContainer() {
                overheadTicks = overheadTicks,
                totalTicks = samples.Max(t => t.ticksTotal),
                samples = samples
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
            var path = string.Format(Settings.Instance.timingsPath, DateTime.Now);
            var folder = Path.GetDirectoryName(path);

            if(!string.IsNullOrEmpty(folder) && !Directory.Exists(Path.GetDirectoryName(path)))
                Directory.CreateDirectory(folder);

            return path;
        }

    }

}