using System;
#if UNITY_EDITOR
using UnityEditor;
#endif
using GrandTheftAuto.Diagnostics;
using GrandTheftAuto.Shared;

namespace GrandTheftAuto {
    public class ProgressBar : IDisposable {
        public int Count { get; set; }
        public int Current { get; set; }
        public int Step { get; set; }
        public string Name { get; set; }
        public bool Canceled { get; set; }
        public TempWorkingFolder WorkingFolder { get; private set; }

        public ProgressBar(string name, int count) {
            Name = name;
            Count = count;
            Step = 1;
        }

        public ProgressBar(string name, int count, TempWorkingFolder workingFolder) {
            Name = name;
            Count = count;
            Step = 1;
            WorkingFolder = workingFolder;
        }

        public ProgressBar(string name, int count, int step) {
            Name = name;
            Count = count;
            Step = step;
        }

        public ProgressBar(string name, int count, TempWorkingFolder workingFolder, int step) {
            Name = name;
            Count = count;
            Step = step;
            WorkingFolder = workingFolder;
        }

        public bool Increment(string info) {
            Current++;

            if(Current % Step != 0)
                return Canceled;

            using(Timing.Get("Progress Bar")) {
                #if UNITY_EDITOR
                var folder = (TempWorkingFolder)null;

                if(WorkingFolder != null)
                    folder = WorkingFolder.Restore();

                if(EditorUtility.DisplayCancelableProgressBar(string.Format("{0} ({1}/{2})", Name, Current, Count), info, (float)Current / Count))
                    Canceled = true;

                if(folder != null)
                    folder.Dispose();
                #endif

                return Canceled;
            }
        }

        public void Dispose() {
            #if UNITY_EDITOR
            var folder = (TempWorkingFolder)null;

            if(WorkingFolder != null)
                folder = WorkingFolder.Restore();

            EditorUtility.ClearProgressBar();

            if(folder != null)
                folder.Dispose();
            #endif
        }
    }
}