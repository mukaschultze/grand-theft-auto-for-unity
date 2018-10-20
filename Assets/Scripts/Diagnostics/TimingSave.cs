using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using GrandTheftAuto.Shared;
using GrandTheftAuto.Shared;
using UnityEngine;

namespace GrandTheftAuto.Diagnostics {

    [Serializable]
    public struct TimingSaved {
        public long totalTicks;
        public long overheadTicks;
        public TimingData[] data;
    }

    public static class TimingSave {

        private static Action<TimingSaved> m_onDump = (t) => { };

        public static Action<TimingSaved> OnDump {
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

        public static void Dump(long overheadTicks, TimingData[] data) {

            var toSave = new TimingSaved() {
                overheadTicks = overheadTicks,
                totalTicks = data.Max(t => t.ticksTotal),
                data = data
            };

            var path = GetNewFileName();
            var json = JsonUtility.ToJson(toSave, true);

            File.WriteAllText(path, json);
            OnDump(toSave);
        }

        public static TimingSaved Load(string path) {
            var json = File.ReadAllText(path);
            var obj = JsonUtility.FromJson<TimingSaved>(json);

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