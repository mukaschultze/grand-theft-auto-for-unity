using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GrandTheftAuto.Diagnostics;

namespace GrandTheftAuto.Img {
    public class ImgFile : IEnumerable<FileEntry> {
        public const string IMG_MAIN = "models/gta3.img";
        private const int VER2 = 844252502; //VER2 in ASCII

        private bool loaded;
        private int entriesCount;
        private FileEntry[] entriesArray;
        private Dictionary<string, FileEntry> entries;
        private BufferReader reader;

        public int EntriesCount { get { if(!loaded) LoadEntries(); return entriesCount; } }
        public string FilePath { get { return ArchiveFile.FilePath; } }
        public GtaVersion Version { get; private set; }

        public FileEntry ArchiveFile { get; private set; }
        public FileEntry[] Entries { get { if(!loaded) LoadEntries(); return entriesArray; } }
        public FileEntry this[int index] { get { if(!loaded) LoadEntries(); return entriesArray[index]; } }
        public FileEntry this[string fileName] { get { if(!loaded) LoadEntries(); return entries[fileName]; } }

        public ImgFile(string path) : this(new FileEntry(path)) { }

        public ImgFile(string path, GtaVersion version) : this(new FileEntry(path), version) { }

        public ImgFile(FileEntry file) {
            ArchiveFile = file;
            Version = TryGetVersion();
            Log.Message("No version specified for \"{0}\", loaded {1}", FilePath, Version.GetFormatedGTAName(false));
        }

        public ImgFile(FileEntry file, GtaVersion version) {
            ArchiveFile = file;
            Version = version;
        }

        private void LoadEntries() {
            using(new Timing("Loading Entries")) {
                switch(Version) {
                    case GtaVersion.III:
                    case GtaVersion.ViceCity:
                        var dirPath = Path.ChangeExtension(ArchiveFile.FilePath, "dir");

                        if(!File.Exists(dirPath))
                            throw new FileNotFoundException(string.Format("There should be a .dir file along the \"{0}\" file", ArchiveFile.FileName), ArchiveFile.FileName);

                        reader = new BufferReader(new FileStream(dirPath, FileMode.Open));
                        entriesCount = (int)reader.Length / 32;
                        break;

                    case GtaVersion.SanAndreas:
                        reader = ArchiveFile.Reader;
                        reader.PrewarmBuffer(8);

                        if(reader.ReadInt32() != VER2)
                            throw new FileLoadException("Incorrect Img archive for GTA San Andreas, expected img archive version 2");

                        entriesCount = reader.ReadInt32();
                        break;

                    default:
                        throw new FileLoadException("Version not recognized");
                }

                entries = new Dictionary<string, FileEntry>(entriesCount, StringComparer.OrdinalIgnoreCase);
                reader.PrewarmBuffer(entriesCount * 32);

                for(var i = 0; i < entriesCount; i++) {
                    var pos = reader.ReadInt32() * 2048;
                    var length = reader.ReadInt32() * 2048;
                    var name = reader.ReadBytes(24).GetNullTerminatedString();

                    try { entries.Add(name, new FileEntry(ArchiveFile, name, pos, length)); }
                    catch { Log.Error("Duplicated entry name \"{0}\" in \"{1}\"", name, FilePath); }
                }

                if(entries.Count != entriesCount)
                    Log.Warning("Expected {0} entries, found {1} at \"{2}\"", entriesCount, entries.Count, FilePath);
                else
                    Log.Message("Loaded {0} entries at \"{1}\"", entriesCount, FilePath);

                if(Version < GtaVersion.SanAndreas)
                    reader.Dispose(); //Disposing de DIR file, not the IMG

                entriesArray = entries.Values.ToArray();
                loaded = true;
            }
        }

        private GtaVersion TryGetVersion() {
            return ArchiveFile.Reader.ReadInt32() == VER2 ? GtaVersion.SanAndreas : GtaVersion.ViceCity;
        }

        public static ImgFile GetMainImg() {
            return new ImgFile(IMG_MAIN);
        }

        public static ImgFile GetMainImg(string gtaPath) {
            return new ImgFile(Path.Combine(gtaPath, IMG_MAIN));
        }

        public static ImgFile GetMainImg(GtaVersion version) {
            var gtaPath = Directories.GetPathFromVersion(version);
            return new ImgFile(Path.Combine(gtaPath, IMG_MAIN), version);
        }

        IEnumerator<FileEntry> IEnumerable<FileEntry>.GetEnumerator() {
            return ((IEnumerable<FileEntry>)Entries).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return ((IEnumerable<FileEntry>)Entries).GetEnumerator();
        }
    }
}