using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace GrandTheftAuto.Shared {
    public partial class Settings : ISerializationCallbackReceiver {

        public string gtaIII;
        public string gtaVC;
        public string gtaSA;

        public int numberOfThreads;

        public bool gpuDecoding = false;
        public bool useMipmaps = false;
        public bool compressTextures = true;
        public bool stackTraceEnabled = true;
        public string logFilePath;
        public string timingsPath;
        public string fileBrowser;

        public Settings() {
            numberOfThreads = SystemInfo.processorCount;
            logFilePath = Path.Combine("log.txt");
            timingsPath = Path.Combine("Timings", "timing_{0:yyyy_MM_dd_HH_mm_ss}.json");
            fileBrowser = Path.Combine(Application.streamingAssetsPath, "FileBrowser", "FileBrowser.exe");
        }

        public void OnBeforeSerialize() { }

        public void OnAfterDeserialize() { }

    }
}