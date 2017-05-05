using System;
using GrandTheftAuto.Diagnostics;

namespace GrandTheftAuto {
    public class TempWorkingFolder : IDisposable {

        public string OlderFolder { get; private set; }
        public string CurrentFolder { get; private set; }

        private bool log;

        public TempWorkingFolder(string folder) : this(folder, true) { }

        private TempWorkingFolder(string folder, bool log) {
            OlderFolder = Environment.CurrentDirectory;
            Environment.CurrentDirectory = CurrentFolder = folder;

            if(this.log = log)
                Log.Message("Set working folder to \"{0}\"", folder);
        }

        public void Dispose() {
            Environment.CurrentDirectory = OlderFolder;

            if(log)
                Log.Message("Reset working folder to \"{0}\"", OlderFolder);
        }

        public TempWorkingFolder Restore() {
            return new TempWorkingFolder(OlderFolder, false);
        }
    }
}