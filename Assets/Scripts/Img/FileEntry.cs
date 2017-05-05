using System.Collections.Generic;
using System.IO;
using GrandTheftAuto.Diagnostics;

namespace GrandTheftAuto.Img {
    public class FileEntry {
        public string FileName { get; private set; }
        public string FileNameWithoutExtension { get { return Path.GetFileNameWithoutExtension(FileName); } }
        public string FilePath { get; private set; }
        public int Offset { get; private set; }
        public int Size { get; private set; }
        public BufferReader Reader {
            get {
                reader.Position = Offset;
                return reader;
            }
        }

        private readonly BufferReader reader;
        private static readonly Dictionary<BufferReader, int> dependents = new Dictionary<BufferReader, int>();

        public FileEntry(string filePath) {
            using(new Timing("Creating (from filepath)")) {
                Offset = 0;
                Size = (int)new FileInfo(filePath).Length;
                FilePath = filePath;
                FileName = Path.GetFileName(FilePath);
                reader = new BufferReader(new FileStream(FilePath, FileMode.Open));
                dependents[reader] = 1;
            }
        }

        public FileEntry(FileEntry file, string virtualFileName, int offset, int size) {
            using(new Timing("Creating (from parent)")) {
                Size = size;
                Offset = file.Offset + offset;
                FilePath = file.FilePath;
                FileName = virtualFileName;
                reader = file.reader;
                dependents[reader]++;
            }
        }

        ~FileEntry() {
            if(--dependents[reader] > 0)
                return;

            try {
                dependents.Remove(reader);
                reader.Dispose();
                Log.Message("Closed stream for file: {0}", FilePath);
            }
            catch { Log.Error("Failed to close stream for file: {0}", FilePath); }
        }

        public byte[] GetData() {
            using(new Timing("Data gathering")) {
                reader.Position = Offset;
                return reader.ReadBytes(Size);
            }
        }
    }
}