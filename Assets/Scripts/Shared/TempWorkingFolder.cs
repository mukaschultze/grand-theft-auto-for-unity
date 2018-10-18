using System;
using GrandTheftAuto.Diagnostics;

namespace GrandTheftAuto.Shared {
    public class TempWorkingFolder : IDisposable {

        public string CurrentDirectory { get; private set; }
        public string PreviousDirectory { get; private set; }

        private bool logOnChange;

        public TempWorkingFolder(string directory) : this(directory, true) { }

        private TempWorkingFolder(string directory, bool logOnChange) {
            PreviousDirectory = Environment.CurrentDirectory;
            Environment.CurrentDirectory = CurrentDirectory = directory;

            if(this.logOnChange = logOnChange)
                Log.Message("Set working directory to \"{0}\"", directory);
        }

        public void Dispose() {
            Environment.CurrentDirectory = PreviousDirectory;

            if(logOnChange)
                Log.Message("Reset working directory to \"{0}\"", PreviousDirectory);
        }

        public TempWorkingFolder Restore() {
            return new TempWorkingFolder(PreviousDirectory, false);
        }

    }
}