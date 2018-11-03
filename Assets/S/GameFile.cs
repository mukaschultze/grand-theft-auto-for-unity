using System;
using System.IO;

namespace GrandTheftAuto.New {
    public abstract class GameFile {

        public FileEntry fileEntry;
        private bool loaded;

        public GameFile(string filePath) {
            this.fileEntry = new FileEntry(filePath);
        }

        public GameFile(FileEntry fileEntry) {
            this.fileEntry = fileEntry;
        }

        public void Load() {
            if(loaded)
                return;

            loaded = true;
            Reload();
        }

        public abstract void Reload();

    }
}