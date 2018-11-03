using System;
using System.Collections.Generic;
using System.IO;
using GrandTheftAuto.Diagnostics;

namespace GrandTheftAuto.New {
    [Serializable]
    public struct FileEntry {

        public string virtualName;
        public string virtualNameWithoutExtension;
        public string basePath;

        public int offset;
        public int size;

        public FileEntry(string filePath) {
            offset = 0;
            size = (int)new FileInfo(filePath).Length;

            this.basePath = filePath;

            try {
                this.virtualName = Path.GetFileName(filePath);
                this.virtualNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);
            } catch {
                this.virtualName = filePath;
                this.virtualNameWithoutExtension = virtualName;
                UnityEngine.Debug.LogError(filePath);
            }
        }

        public FileEntry(FileEntry parent, string virtualName, int offset, int size) {
            this.size = size;
            this.offset = parent.offset + offset;
            this.basePath = parent.basePath;
            try {
                this.virtualName = virtualName;
                this.virtualNameWithoutExtension = Path.GetFileNameWithoutExtension(virtualName);
            } catch {
                this.virtualName = virtualName;
                this.virtualNameWithoutExtension = virtualName;
                UnityEngine.Debug.LogError(virtualName);
            }
        }

        public Stream GetReadStream() {
            var stream = File.Open(basePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            stream.Seek(offset, SeekOrigin.Begin);
            return stream;
        }

        public Stream GetWriteStream(bool create = false) {
            var stream = File.Open(basePath, create? FileMode.OpenOrCreate : FileMode.Open, FileAccess.Write, FileShare.Read);
            stream.Seek(offset, SeekOrigin.Begin);
            return stream;
        }

        public Stream GetReadWriteStream(bool create = false) {
            var stream = File.Open(basePath, create? FileMode.OpenOrCreate : FileMode.Open, FileAccess.ReadWrite, FileShare.Read);
            stream.Seek(offset, SeekOrigin.Begin);
            return stream;
        }

        public void CopyTo(string filePath) {
            using(var readStream = GetReadStream())
            using(var writeStream = File.OpenWrite(filePath)) {
                var buffer = new byte[size];

                readStream.Read(buffer, 0, size);
                writeStream.Write(buffer, 0, size);
            }
        }

        public override string ToString() {
            return virtualName;
        }

    }
}