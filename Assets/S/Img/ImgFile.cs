using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GrandTheftAuto.Diagnostics;
using GrandTheftAuto.Shared;

namespace GrandTheftAuto.New {
    public class ImgFile : GameFile {

        public const string IMG_MAIN = "models/gta3.img";
        private const int VER2 = 844252502; //VER2 in ASCII

        public FileEntry[] files;
        public GtaVersion version;

        public ImgFile(string filePath) : base(filePath) {
            this.version = TryGetVersion();
            Log.Message("No version specified for \"{0}\", loaded {1}", fileEntry.basePath, version.GetFormatedGTAName(false));
        }

        public ImgFile(string filePath, GtaVersion version) : base(filePath) {
            this.version = version;
        }

        public ImgFile(FileEntry fileEntry) : base(fileEntry) {
            this.version = TryGetVersion();
            Log.Message("No version specified for \"{0}\", loaded {1}", base.fileEntry.basePath, version.GetFormatedGTAName(false));
        }

        public ImgFile(FileEntry fileEntry, GtaVersion version) : base(fileEntry) {
            this.version = version;
        }

        public override void Reload() {
            using(Timing.Get("Loading Entries")) {

                var buffer = (UnmanagedBuffer)null;
                var entriesCount = 0;

                switch(version) {
                    case GtaVersion.III:
                    case GtaVersion.ViceCity:
                        try {
                            var dirPath = Path.ChangeExtension(fileEntry.basePath, "dir");

                            buffer = new UnmanagedBuffer(dirPath);
                            entriesCount = buffer.StreamLength / 32;
                        } catch(FileNotFoundException e) {
                            throw new FileNotFoundException("There should be a .dir file along .img file", e.FileName);
                        }
                        break;

                    case GtaVersion.SanAndreas:
                        buffer = new UnmanagedBuffer(fileEntry);
                        var reader = buffer.GetReader(8);

                        if(reader.ReadInt32() != VER2)
                            throw new FileLoadException("Incorrect Img archive for GTA San Andreas, expected img archive version 2");

                        entriesCount = reader.ReadInt32();
                        break;

                    default:
                        throw new FileLoadException("Version not recognized");
                }

                using(buffer) {
                    files = new FileEntry[entriesCount];
                    var reader = buffer.GetReader(entriesCount * 32);

                    ThreadUtility.For(entriesCount, (i) => {
                        var pReader = reader.Split(i, entriesCount);
                        var pos = pReader.ReadInt32() * 2048;
                        var length = pReader.ReadInt32() * 2048;
                        var name = pReader.ReadString(24);
                        var entry = new FileEntry(fileEntry, name, pos, length);

                        files[i] = entry;
                    });
                }
            }
        }

        private GtaVersion TryGetVersion() {
            using(var buffer = new UnmanagedBuffer(fileEntry)) {
                var reader = buffer.GetReader(4).ReadInt32();
                Log.Message(buffer.buffer.Length);
                Log.Message(reader);
                Log.Message(fileEntry.offset);
                Log.Message(fileEntry.size);
                return reader == VER2 ? GtaVersion.SanAndreas : GtaVersion.ViceCity;
            }
        }

        public static ImgFile GetMainImg() {
            return new ImgFile(IMG_MAIN);
        }

        public static ImgFile GetMainImg(string gtaPath) {
            return new ImgFile(Path.Combine(gtaPath, IMG_MAIN));
        }

        public static ImgFile GetMainImg(GtaVersion version) {
            return new ImgFile(GetMainImgPath(version), version);
        }

        public static string GetMainImgPath(GtaVersion version) {
            var gtaPath = Directories.GetPathFromVersion(version);
            return Path.Combine(gtaPath, IMG_MAIN);
        }

    }
}